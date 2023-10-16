// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Generators;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;
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
    private readonly IReadOnlyCollection<IDataExporter> _exporters;
    private readonly IReadOnlyCollection<GeneratorDescriptor> _descriptors;

    private Timer _workerTimer;
    private readonly Stopwatch _stopwatch;

    private const string LastDatabaseVersionKey = "onionfruit-web-worker:dbversion";

    public Worker(IServiceScopeFactory ssf, IConfiguration config, ILogger<Worker> logger)
    {
        _ssf = ssf;
        _logger = logger;
        _stopwatch = new Stopwatch();

        _exporters = GetExporters(config);
        _descriptors = GetDescriptors(config);
    }

    private async Task PerformUpdate()
    {
        using var scope = _ssf.CreateScope();

        var dataSourceUpdated = false;
        var sourceInstances = new Dictionary<Type, IDataSource>();

        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var nextVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

#if !DEBUG
        var lastUpdatedValue = await redis.StringGetAsync(LastDatabaseVersionKey).ConfigureAwait(false);
        var lastVersion = DateTimeOffset.FromUnixTimeSeconds(lastUpdatedValue.HasValue && long.TryParse(lastUpdatedValue.ToString(), out var val) ? val : 0);
#else
        // in debug mode, use minvalue to always perform fetch.
        var lastVersion = DateTimeOffset.MinValue;
#endif

        _stopwatch.Restart();

        // create instances of all source types and set switch if data source has been updated
        foreach (var sourceType in _descriptors.SelectMany(x => x.SourceTypes).Distinct())
        {
            var source = (IDataSource)ActivatorUtilities.CreateInstance(scope.ServiceProvider, sourceType);

            sourceInstances[sourceType] = source;
            dataSourceUpdated |= await source.HasDataChanged(lastVersion).ConfigureAwait(false);
        }

        if (!dataSourceUpdated)
        {
            _stopwatch.Stop();
            _logger.LogInformation("No sources have been updated. Generator execution skipped (after {ts}).", _stopwatch.Elapsed);
            return;
        }

        // fetch all data sources
        await Task.WhenAll(sourceInstances.Select(x => x.Value.CollectData())).ConfigureAwait(false);

        // file sink used to store static-generated assets for uploading to s3 or saving to a local path
        using var fileSink = new FileSink();

        foreach (var generatorDescriptor in _descriptors)
        {
            IDisposable disposableGeneratorInstance = null;

            try
            {
                _logger.LogInformation("Running generator for {name}...", generatorDescriptor.OutputFormat.Name);

                var instanceSources = generatorDescriptor.SourceTypes.Select(x => (object)sourceInstances[x]).ToArray();
                var generatorInstance = (IDatabaseGenerator)ActivatorUtilities.CreateInstance(scope.ServiceProvider, generatorDescriptor.OutputFormat, instanceSources);

                disposableGeneratorInstance = generatorInstance as IDisposable;

                await generatorInstance.GenerateDatabase(fileSink).ConfigureAwait(false);
                _logger.LogInformation("Generator finished successfully");
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Database Generator {x} has failed: {err}", generatorDescriptor.OutputFormat.Name, e.Message);
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

        // upload files
        if (fileSink.HasItems)
        {
            foreach (var exporter in _exporters)
            {
                _logger.LogInformation("Exporting to {dest}", exporter);

                await exporter.PerformUpload(scope.ServiceProvider, fileSink).ConfigureAwait(false);
                _logger.LogDebug("Export completed successfully");
            }
        }

        _stopwatch.Stop();
        _logger.LogInformation("Worker update completed successfully (took {ts})", _stopwatch.Elapsed);

        await redis.StringSetAsync(LastDatabaseVersionKey, nextVersion, TimeSpan.FromDays(1)).ConfigureAwait(false);
    }

    private IReadOnlyCollection<GeneratorDescriptor> GetDescriptors(IConfiguration config)
    {
        var listing = new List<GeneratorDescriptor>();

        // get all file generators, determine what generators need what types
        foreach (var fileGeneratorType in GetType().Assembly.ExportedTypes.Where(x => x.IsAssignableTo(typeof(IDatabaseGenerator))))
        {
            if (config.GetSection("EnabledGenerators").GetValue<string>(fileGeneratorType.Name)?.Equals("false", StringComparison.OrdinalIgnoreCase) == true)
            {
                _logger.LogInformation("{gen} was disabled by configuration", fileGeneratorType.Name);
                continue;
            }

            var ctor = fileGeneratorType.GetConstructors().SingleOrDefault();

            if (ctor == null)
            {
                // IDatabaseGenerators must have a single constructor. if they don't, ignore it.
                continue;
            }

            var paramTypes = ctor.GetParameters()
                .Where(x => x.ParameterType is {IsAbstract: false, IsInterface: false} && x.ParameterType.IsAssignableTo(typeof(IDataSource)))
                .Select(x => x.ParameterType);

            listing.Add(new GeneratorDescriptor(fileGeneratorType, paramTypes.ToList()));
        }

        return listing;
    }

    private static IReadOnlyCollection<IDataExporter> GetExporters(IConfiguration config)
    {
        var exporters = new List<IDataExporter>();

        foreach (var section in config.GetSection("Exports").GetChildren())
        {
            IDataExporter entity = section["Type"]?.ToUpperInvariant() switch
            {
                "FOLDER" => new FolderExporter(),
                "URL" => new RemoteArchiveExporter(),

                _ => null
            };

            if (entity == null)
            {
                continue;
            }

            section.Bind(entity);
            exporters.Add(entity);
        }

        return exporters;
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _workerTimer?.Dispose();
        _workerTimer = new Timer(_ => PerformUpdate(), null, TimeSpan.Zero, TimeSpan.FromHours(12));

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _workerTimer?.Dispose();
        return Task.CompletedTask;
    }
}