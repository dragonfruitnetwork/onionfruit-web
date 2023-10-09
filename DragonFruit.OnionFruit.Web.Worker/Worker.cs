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
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker;

public class Worker : IHostedService
{
    private record GeneratorDescriptor(Type OutputFormat, IReadOnlyList<Type> SourceTypes);

    private readonly ILogger<Worker> _logger;
    private readonly IServiceScopeFactory _ssf;
    private readonly IReadOnlyCollection<GeneratorDescriptor> _descriptors;

    private Timer _workerTimer;

    private const string LastDatabaseVersionKey = "onionfruit-web-worker:dbversion";

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

        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var nextVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

#if !DEBUG
        var lastUpdatedValue = await redis.StringGetAsync(LastDatabaseVersionKey).ConfigureAwait(false);
        var lastVersion = DateTimeOffset.FromUnixTimeSeconds(lastUpdatedValue.HasValue && long.TryParse(lastUpdatedValue.ToString(), out var val) ? val : 0);
#else
        // in debug mode, use minvalue to always perform fetch.
        var lastVersion = DateTimeOffset.MinValue;
#endif

        // populate list with the sources that have been updated since last check
        foreach (var sourceType in _descriptors.SelectMany(x => x.SourceTypes).Distinct())
        {
            var source = (IDataSource)ActivatorUtilities.CreateInstance(scope.ServiceProvider, sourceType);

            if (await source.HasDataChanged(lastVersion).ConfigureAwait(false))
            {
                sourcesTypesToUse.Add(sourceType);
            }

            sourceInstances[sourceType] = source;
        }

        if (!sourcesTypesToUse.Any())
        {
            _logger.LogInformation("No sources have been updated. Generator execution skipped.");
            return;
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
            _logger.LogInformation("Collecting data for {sources}", string.Join(", ", sourcesTypesToUse.Select(x => x.Name)));
            await Task.WhenAll(sourcesTypesToUse.Select(x => sourceInstances[x].CollectData())).ConfigureAwait(false);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to collect data for one or more sources: {message}", e.Message);
            return;
        }

        // file sink used to store static-generated assets for uploading to s3 or saving to a local path
        var fileSink = new DatabaseFileSink();

        foreach (var generator in generatorsToUse)
        {
            IDisposable disposableGeneratorInstance = null;
            
            try
            {
                _logger.LogInformation("Running generator for {name}...", generator.OutputFormat.Name);

                var instanceSources = generator.SourceTypes.Select(x => (object)sourceInstances[x]).ToArray();
                var generatorInstance = (IDatabaseGenerator)ActivatorUtilities.CreateInstance(scope.ServiceProvider, generator.OutputFormat, instanceSources);

                disposableGeneratorInstance = generatorInstance as IDisposable;
                
                await generatorInstance.GenerateDatabase(fileSink).ConfigureAwait(false);
                _logger.LogInformation("Generator finished successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Database Generator {x} has failed: {err}", generator.OutputFormat.Name, e.Message);
            }
            finally
            {
                disposableGeneratorInstance?.Dispose();
            }
        }

        _logger.LogInformation("Database processing completed");
        foreach (var item in sourceInstances.Values.OfType<IDisposable>())
        {
            item.Dispose();
        }

        // upload files todo iterate all sources from config and perform persistance actions.
        if (fileSink.HasItems)
        {
        }

        await redis.StringSetAsync(LastDatabaseVersionKey, nextVersion, TimeSpan.FromDays(1)).ConfigureAwait(false);
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
