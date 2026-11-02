// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Reflection;
using Amazon.Runtime;
using Amazon.S3;
using Coravel;
using Coravel.Scheduling.Schedule.Interfaces;
using DragonFruit.OnionFruit.AssetGenerator.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources;
using DragonFruit.OnionFruit.AssetGenerator.Publishing;
using DragonFruit.OnionFruit.AssetGenerator.Worker.Configuration;
using DragonFruit.OnionFruit.AssetGenerator.Worker.Services;
using DragonFruit.OnionFruit.Redis;
using Microsoft.Extensions.Options;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Logging
            .ClearProviders()
            .AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.IncludeScopes = false;
                o.TimestampFormat = "[dd/MM/yyyy hh:mm:ss] ";
            })
            .AddSentry(o =>
            {
                o.Dsn = builder.Configuration.GetSection("Sentry").Get<WorkerSentryConfig>()?.Dsn ?? string.Empty;

                o.MaxBreadcrumbs = 50;
                o.MinimumEventLevel = LogLevel.Error;
                o.MinimumBreadcrumbLevel = LogLevel.Debug;

                o.Release = Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "0.1";
            });

        builder.Services.AddAssetGeneration(options => builder.Configuration.GetSection("AssetGeneration").Bind(options));
        builder.Services.AddAssetGeneratorSources();
        builder.Services.AddAssetGeneratorGenerators();

        builder.Services.AddOptions<WorkerOptions>()
            .Bind(builder.Configuration.GetSection(WorkerOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOptions<RedisOptions>()
            .Bind(builder.Configuration.GetSection(RedisOptions.SectionName));

        builder.Services.AddOptions<S3AssetPublisherOptions>()
            .Bind(builder.Configuration.GetSection(S3AssetPublisherOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddOnionFruitRedis();
        builder.Services.AddSingleton<IAssetGenerationStateStore, RedisAssetGenerationStateStore>();

        builder.Services.AddSingleton<IAmazonS3>(services =>
        {
            var options = services.GetRequiredService<IOptions<S3AssetPublisherOptions>>().Value;
            var configuration = new AmazonS3Config
            {
                ServiceURL = options.Endpoint,
                AuthenticationRegion = options.Region,

                ForcePathStyle = true,
            };

            return new AmazonS3Client(new BasicAWSCredentials(options.AccessKey, options.SecretKey), configuration);
        });

        builder.Services.AddSingleton<IAssetPublisher, S3AssetPublisher>();
        builder.Services.AddSingleton<IAssetPublisher, RedisAssetIndexPublisher>();
        builder.Services.AddTransient<AssetGenerationJob>();
        builder.Services.AddTransient<AssetExpiryPurgeJob>();

        builder.Services.AddScheduler();

        var host = builder.Build();
        var workerOptions = host.Services.GetRequiredService<IOptions<WorkerOptions>>().Value;
        var schedulerLogger = host.Services.GetRequiredService<ILoggerFactory>().CreateLogger("AssetGenerationScheduler");

        host.Services.UseScheduler(s =>
        {
            (s as ISchedulerConfiguration)?.OnError(exception => schedulerLogger.LogError(exception, "Scheduled asset generation failed"));

            var assetGeneratorJob = s.Schedule<AssetGenerationJob>().DailyAtHour(4).PreventOverlapping("asset-generation");

            if (workerOptions.RunOnStartup)
            {
                assetGeneratorJob.RunOnceAtStart();
            }

            if (workerOptions.PurgeExpiredAssets)
            {
                s.Schedule<AssetExpiryPurgeJob>().Cron("0 1 * * 5").PreventOverlapping("asset-expiry-purge").RunOnceAtStart();
            }
        });

        await RedisIndexInitializer.InitializeAsync(host.Services);
        await host.RunAsync();
    }
}