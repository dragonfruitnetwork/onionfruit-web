using System;
using System.Text.Json;
using System.Threading.Tasks;
using DnsClient;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Web.Data;
using DragonFruit.OnionFruit.Web.Worker;
using libloc.Access;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configPathBase = Environment.GetEnvironmentVariable("CONFIG_FOLDER_PATH");

        if (!string.IsNullOrEmpty(configPathBase))
        {
            builder.Configuration.SetBasePath(configPathBase).AddJsonFile("appsettings.json");
        }

        Worker.Program.ConfigureLogging(builder.Logging, builder.Configuration, "Server");

        builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy =>
        {
            policy.WithMethods("GET");
            policy.SetPreflightMaxAge(TimeSpan.FromHours(12));

            policy.SetIsOriginAllowed(s =>
            {
                var uri = new Uri(s, UriKind.Absolute);
                return uri.Host == "localhost" || uri.Host.Equals("dragonfruit.network", StringComparison.OrdinalIgnoreCase)
                                               || uri.Host.EndsWith(".dragonfruit.network", StringComparison.OrdinalIgnoreCase);
            });
        }));

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedForHeaderName = "CF-Connecting-IP";
            options.ForwardedProtoHeaderName = "X-Forwarded-Proto";
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        // databases
        builder.Services.AddLocationDb();
        builder.Services.AddSingleton(_ => RedisClientConfigurator.CreateConnectionMultiplexer(builder.Configuration, false));
        builder.Services.AddSingleton<IRedisConnectionProvider>(s => new RedisConnectionProvider(s.GetRequiredService<IConnectionMultiplexer>()));

        // api clients
        builder.Services.AddSingleton<ILookupClient, LookupClient>();
        builder.Services.AddSingleton<ApiClient, WorkerApiClient>();

        builder.Services.AddSingleton<LocalAssetStore>();

        // register worker if running in integrated mode (worker copies files out
        // cannot run both the integrated worker and the remote asset fetcher at same time due to different versioning systems used.
        if (builder.Configuration["Worker:Enabled"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            builder.Services.AddSingleton<Worker.Worker>();
            builder.Services.AddHostedService(s => s.GetRequiredService<Worker.Worker>());
        }
        else
        {
            builder.Services.AddHostedService<RemoteAssetFetcher>();
        }

        var app = builder.Build();

#if !DEBUG
        app.UseForwardedHeaders();
#else
        app.UseDeveloperExceptionPage();
#endif

        app.UseStaticFiles();
        app.UseRouting();
        app.UseCors();

        app.MapControllers();

        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            // setup local file watchers and orphaned file task
            // as the assetstore is registered in the dependency container watchers will be started immediately after instance creation
            scope.ServiceProvider.GetRequiredService<LocalAssetStore>().StartWatchers();

            // if worker is enabled, add local exporter and run redis migrations
            var worker = scope.ServiceProvider.GetService<Worker.Worker>();

            if (worker != null)
            {
                worker.AddExporter(new LocalWorkerExporter());
                await Worker.Program.ValidateRedisStructures(scope.ServiceProvider).ConfigureAwait(false);
            }
        }

        await app.RunAsync().ConfigureAwait(false);
    }
}