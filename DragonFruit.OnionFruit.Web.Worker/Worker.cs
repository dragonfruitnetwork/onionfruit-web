// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Configuration;
using DragonFruit.OnionFruit.Web.Worker.Generators;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker;

public class Worker : IHostedService
{
    private record GeneratorDescriptor(Type OutputFormat, IReadOnlyList<Type> SourceTypes);

    private readonly Stopwatch _stopwatch;
    private readonly RedisOptions _redisOptions;
    private readonly ILogger<Worker> _logger;
    private readonly IDataExporter _exporter;
    private readonly IServiceScopeFactory _ssf;
    private readonly IReadOnlyCollection<GeneratorDescriptor> _descriptors;

    private Timer _workerTimer;

    private const string LastDatabaseVersionKey = "dbversion";

    public Worker(IServiceScopeFactory ssf, IOptions<RedisOptions> redisOptions, IOptions<WorkerOptions> workerOptions, IDataExporter exporter, ILogger<Worker> logger)
    {
        _ssf = ssf;
        _logger = logger;
        _exporter = exporter;
        _redisOptions = redisOptions.Value;
        _stopwatch = new Stopwatch();
        _descriptors = GetDescriptors(workerOptions.Value);
    }

    private async Task PerformUpdate()
    {
        using var scope = _ssf.CreateScope();

        var dataSourceUpdated = false;
        var sourceInstances = new Dictionary<Type, IDataSource>();

        var redis = scope.ServiceProvider.GetRequiredService<IConnectionMultiplexer>().GetDatabase();
        var keyPrefix = _redisOptions.KeyPrefix;

        var databaseVersionKey = new RedisKey($"{keyPrefix}:{LastDatabaseVersionKey}");
        var nextVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

#if !DEBUG
        var lastUpdatedValue = await redis.StringGetAsync(databaseVersionKey).ConfigureAwait(false);
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

            _logger.LogInformation("Checking if {source} has been updated", sourceType.Name);

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
        _logger.LogInformation("Waiting for {count} sources to be fetched...", sourceInstances.Count);
        await Task.WhenAll(sourceInstances.Select(x => x.Value.CollectData())).ConfigureAwait(false);

        // file sink used to store static-generated assets for uploading to s3 or saving to a local path
        using var fileSink = new FileSink(nextVersion.ToString(CultureInfo.InvariantCulture));

        foreach (var generatorDescriptor in _descriptors)
        {
            IDisposable disposableGeneratorInstance = null;

            try
            {
                _logger.LogInformation("Running {name}...", generatorDescriptor.OutputFormat.Name);

                var instanceSources = generatorDescriptor.SourceTypes.Select(x => (object)sourceInstances[x]).ToArray();
                var generatorInstance = (IDatabaseGenerator)ActivatorUtilities.CreateInstance(scope.ServiceProvider, generatorDescriptor.OutputFormat, instanceSources);

                disposableGeneratorInstance = generatorInstance as IDisposable;

                await generatorInstance.GenerateDatabase(fileSink).ConfigureAwait(false);
                _logger.LogInformation("{name} finished successfully", generatorDescriptor.OutputFormat.Name);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "{x} has failed: {err}", generatorDescriptor.OutputFormat.Name, e.Message);
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
            try
            {
                _logger.LogInformation("Uploading files to storage...");
                await _exporter.PerformUpload(fileSink).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to upload files: {err}", e.Message);
            }
        }

        _stopwatch.Stop();
        _logger.LogInformation("Worker update completed successfully (took {ts})", _stopwatch.Elapsed);

        await redis.StringSetAsync(databaseVersionKey, nextVersion, TimeSpan.FromDays(1)).ConfigureAwait(false);
    }

    private List<GeneratorDescriptor> GetDescriptors(WorkerOptions options)
    {
        var listing = new List<GeneratorDescriptor>();

        // get all file generators, determine what generators need what types
        foreach (var fileGeneratorType in GetType().Assembly.ExportedTypes.Where(x => x.IsAssignableTo(typeof(IDatabaseGenerator))))
        {
            if (options.EnabledGenerators.TryGetValue(fileGeneratorType.Name, out var generatorEnabled) && !generatorEnabled)
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

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        _workerTimer?.Dispose();
        _workerTimer = new Timer(s => _ = PerformUpdate().ContinueWith(e => _logger.LogError(e.Exception, "Refresh failed: {err}", e.Exception?.Message), TaskContinuationOptions.OnlyOnFaulted), null, TimeSpan.Zero, TimeSpan.FromHours(12));

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _workerTimer?.Dispose();
        return Task.CompletedTask;
    }
}

[JsonSerializable(typeof(CountryMap))]
[JsonSerializable(typeof(TorStatusResponse<TorRelayDetails, TorBridgeDetails>))]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
public partial class WorkerSerializerContext : JsonSerializerContext;
