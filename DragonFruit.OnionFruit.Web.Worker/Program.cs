// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;
using Amazon.Runtime;
using Amazon.S3;
using DnsClient;
using DragonFruit.Data;
using DragonFruit.Data.Serializers;
using DragonFruit.OnionFruit.Web.Worker.Configuration;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Modeling;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureLogging((host, logging) => ConfigureLogging(logging, host.Configuration, "Worker"))
            .ConfigureServices(ConfigureServices)
            .Build();

        using (var scope = host.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            await ValidateRedisStructures(scope.ServiceProvider).ConfigureAwait(false);
        }

        await host.RunAsync().ConfigureAwait(false);
    }

    public static void ConfigureLogging(ILoggingBuilder logging, IConfiguration config, string dsnType)
    {
        logging.ClearProviders();
        logging.AddSimpleConsole(o =>
        {
            o.SingleLine = true;
            o.IncludeScopes = false;
            o.TimestampFormat = $"[dd/MM/yyyy hh:mm:ss] ({dsnType}) ";
        });

#if WINDOWS
        logging.AddEventLog(o =>
        {
            o.Filter = (_, level) => level >= LogLevel.Information;
            o.SourceName = $"OnionFruit-Web-{dsnType}/v{Assembly.GetExecutingAssembly().GetName().Version.ToString(3)}";
        });
#endif

        logging.AddSentry(o =>
        {
            o.Dsn = config[$"{dsnType}:Dsn"] ?? config["Dsn"] ?? string.Empty;
            o.Release = Assembly.GetExecutingAssembly().GetName().Version!.ToString(3);

            o.MaxBreadcrumbs = 50;
            o.MinimumEventLevel = LogLevel.Error;
            o.MinimumBreadcrumbLevel = LogLevel.Debug;
        });
    }

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // configuration
        services.AddRedisOptions(context.Configuration);
        services.AddValidatedOptions<WorkerOptions>(context.Configuration, WorkerOptions.SectionName);
        services.AddValidatedOptions<S3StorageOptions>(context.Configuration, S3StorageOptions.SectionName);

        // redis + redis.om
        services.AddSingleton(s => RedisClientConfigurator.CreateConnectionMultiplexer(s.GetRequiredService<IOptions<RedisOptions>>().Value, true));
        services.AddSingleton<IRedisConnectionProvider>(s => new RedisConnectionProvider(s.GetRequiredService<IConnectionMultiplexer>()));

        services.AddSingleton(s =>
        {
            var storageOptions = s.GetRequiredService<IOptions<S3StorageOptions>>().Value;
            var clientConfig = new AmazonS3Config();

            if (!string.IsNullOrEmpty(storageOptions.Region))
            {
                clientConfig.RegionEndpoint = Amazon.RegionEndpoint.GetBySystemName(storageOptions.Region);
            }
            else if (!string.IsNullOrEmpty(storageOptions.Endpoint))
            {
                clientConfig.ServiceURL = storageOptions.Endpoint;
            }

            return new AmazonS3Client(new BasicAWSCredentials(storageOptions.AccessKey, storageOptions.SecretKey), clientConfig);
        });

        services.AddSingleton<IDataExporter, S3StorageManager>();
        services.AddHostedService(s => (S3StorageManager)s.GetRequiredService<IDataExporter>());

        // api
        services.AddSingleton<ILookupClient, LookupClient>();
        services.AddSingleton<ApiClient>(_ =>
        {
            var client = new ApiClient<ApiJsonSerializer>
            {
                UserAgent = $"OnionFruit-Web-Worker/{typeof(Program).Assembly.GetName().Version?.ToString(3)}"
            };

            client.Serializers.Configure<ApiJsonSerializer>(json =>
            {
                json.SerializerOptions = new JsonSerializerOptions
                {
                    TypeInfoResolverChain = {WorkerSerializerContext.Default}
                };
            });

            return client;
        });

        services.AddHostedService<Worker>();
    }

    public static async Task ValidateRedisStructures(IServiceProvider services)
    {
        var redis = services.GetRequiredService<IRedisConnectionProvider>();
        var logger = services.GetRequiredService<ILogger<IRedisConnectionProvider>>();
        var redisOptions = services.GetRequiredService<IOptions<RedisOptions>>().Value;

        var createNewIndexes = redisOptions.CreateIndexes;
        var regenExistingIndexes = redisOptions.RegenIndexes;

        var documentSourceAssemblies = new[]
        {
            Assembly.GetExecutingAssembly()
        };

        foreach (var type in documentSourceAssemblies.SelectMany(x => x.ExportedTypes).Where(x => x.GetCustomAttribute<DocumentAttribute>() != null))
        {
            // drop index if regenerating
            if (regenExistingIndexes)
            {
                logger.LogInformation("Dropping index for type {t}", type.Name);
                await redis.Connection.DropIndexAsync(type).ConfigureAwait(false);
            }

            // create if enabled OR the index is being regenerated
            if (regenExistingIndexes || createNewIndexes)
            {
                logger.LogInformation("Initialising index for type {t} (if not already present)", type.Name);
                await redis.Connection.CreateIndexAsync(type).ConfigureAwait(false);
            }
        }
    }
}