using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Web.Worker.Generators;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DragonFruit.OnionFruit.Web.Worker;

public class Worker : IHostedService
{
    private record GeneratorDescriptor(Type OutputFormat, IReadOnlyList<Type> SourceTypes);
    
    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _ssf;
    private readonly IReadOnlyCollection<GeneratorDescriptor> _descriptors;

    private Timer _workerTimer;

    private const string DatabaseSinkFileName = "onionfruit-data-{0}.zip";

    public Worker(IServiceScopeFactory ssf, ILogger<Worker> logger)
    {
        _ssf = ssf;
        _logger = logger;
        _descriptors = GetDescriptors();
    }

    private async Task PerformUpdate()
    {
        using var scope = _ssf.CreateScope();

        var sourceInstances = new Dictionary<Type, IDataSource>();
        var sourcesTypesToUse = new HashSet<Type>();

        // populate list with the sources that have been updated since last check
        foreach (var sourceType in _descriptors.SelectMany(x => x.SourceTypes).Distinct())
        {
            var source = (IDataSource)ActivatorUtilities.CreateInstance(scope.ServiceProvider, sourceType);

            if (await source.HasDataChanged(DateTime.MinValue).ConfigureAwait(false))
            {
                sourcesTypesToUse.Add(sourceType);
            }

            sourceInstances[sourceType] = source;
        }
        
        // add any source from full list if a generator that needs an updated source also needs one of the old sources
        var generatorsToUse = _descriptors.Where(x => sourcesTypesToUse.Overlaps(x.SourceTypes)).ToList();
        
        foreach (var sourceType in generatorsToUse.SelectMany(x => x.SourceTypes))
        {
            // hashset doesn't have addrange...
            sourcesTypesToUse.Add(sourceType);
        }
        
        try
        {
            // fetch all sources needed
            await Task.WhenAll(sourcesTypesToUse.Select(x => sourceInstances[x].CollectData())).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to collect data for one or more sources: {message}", e.Message);
            return;
        }

        // file sink used to store static-generated assets for uploading to s3/remote storage using a presigned url.
        var fileSink = new Lazy<IDatabaseFileSink>(() => new DatabaseFileSink());

        foreach (var generator in generatorsToUse)
        {
            try
            {
                var instanceSources = generator.SourceTypes.Select(x => (object)sourceInstances[x]).ToArray();
                var generatorInstance = (IDatabaseGenerator)ActivatorUtilities.CreateInstance(scope.ServiceProvider, generator.OutputFormat, instanceSources);
                
                await generatorInstance.GenerateDatabase(fileSink).ConfigureAwait(false);

                if (generatorInstance is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Database Generator {x} has failed: {err}", generator.OutputFormat.Name, e.Message);
            }
        }
        
        _logger.LogInformation("Database processing completed");
        foreach (var item in sourceInstances.Values.OfType<IDisposable>())
        {
            item.Dispose();
        }
        
        // upload files
        if (fileSink.IsValueCreated)
        {
            await using var archiveStream = ((DatabaseFileSink)fileSink.Value).GetArchive();

            var fileName = string.Format(DatabaseSinkFileName, DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            var uploadUrl = scope.ServiceProvider.GetRequiredService<IConfiguration>()["Storage:UploadUrl"];

            // todo add retry policy to client
            var request = new GenericBlobUploadRequest(uploadUrl, fileName, archiveStream);
            using var response = await scope.ServiceProvider.GetRequiredService<ApiClient>().PerformAsync(request).ConfigureAwait(false);
            
            _logger.LogInformation("{file} uploaded with status code {code}", fileName, response.StatusCode);
        }
    }

    private IReadOnlyCollection<GeneratorDescriptor> GetDescriptors()
    {
        var listing = new List<GeneratorDescriptor>();
        
        // get all file generators, determine what generators need what types
        foreach (var fileGeneratorType in GetType().Assembly.ExportedTypes.Where(x => x.IsAssignableTo(typeof(IDatabaseGenerator))))
        {
            var ctor = fileGeneratorType.GetConstructors().SingleOrDefault();

            if (ctor == null)
            {
                // IDatabaseGenerators must have a single constructor. if they don't, ignore it.
                continue;
            }

            var paramTypes = ctor.GetParameters()
                .Where(x => x.ParameterType is { IsAbstract: false, IsInterface: false } && x.ParameterType.IsAssignableTo(typeof(IDataSource)))
                .Select(x => x.ParameterType);
            
            listing.Add(new GeneratorDescriptor(fileGeneratorType, paramTypes.ToList()));
        }

        return listing;
    }
    
    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _workerTimer?.Dispose();
        _workerTimer = new Timer(_ => PerformUpdate(), null, TimeSpan.Zero, TimeSpan.FromHours(6));

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _workerTimer?.Dispose();
        return Task.CompletedTask;
    }
}