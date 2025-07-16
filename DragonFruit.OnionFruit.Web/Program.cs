using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using DnsClient;
using DragonFruit.Data;
using DragonFruit.Data.Serializers;
using DragonFruit.OnionFruit.Web.Controllers;
using DragonFruit.OnionFruit.Web.Data;
using DragonFruit.OnionFruit.Web.Worker;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Converters;
using DragonFruit.OnionFruit.Web.Worker.Storage;
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

        builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.TypeInfoResolverChain.Insert(0, ControllerSerializerContext.Default));
        builder.Services.AddCors(cors =>
        {
            cors.AddDefaultPolicy(policy =>
            {
                policy.WithMethods("GET");

                policy.SetIsOriginAllowed(IsValidOrigin);
                policy.SetPreflightMaxAge(TimeSpan.FromHours(12));
            });

            cors.AddPolicy("Assets", policy =>
            {
                policy.WithMethods("GET");
                policy.WithHeaders("If-Modified-Since");
                policy.WithExposedHeaders("ETag", "X-Asset-Location");

                policy.SetIsOriginAllowed(IsValidOrigin);
                policy.SetPreflightMaxAge(TimeSpan.FromHours(12));
            });

            return;

            bool IsValidOrigin(string s)
            {
                var uri = new Uri(s, UriKind.Absolute);
                return uri.Host == "localhost" || uri.Host.Equals("dragonfruit.network", StringComparison.OrdinalIgnoreCase)
                                               || uri.Host.EndsWith(".dragonfruit.network", StringComparison.OrdinalIgnoreCase);
            }
        });

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
        builder.Services.AddSingleton<ApiClient>(_ =>
        {
            var client = new ApiClient<ApiJsonSerializer>
            {
                UserAgent = $"OnionFruit-Web-Worker/{typeof(Program).Assembly.GetName().Version?.ToString(3)}"
            };

            client.Serializers.Configure<ApiJsonSerializer>(json =>
            {
                json.SerializerOptions = new JsonSerializerOptions();
                json.SerializerOptions.Converters.Add(new DateTimeConverter());
            });

            return client;
        });

        // register worker if running in integrated mode, using the local asset store for file management
        if (builder.Configuration["Server:UseBuiltInWorker"]?.Equals("false", StringComparison.OrdinalIgnoreCase) != true)
        {
            builder.Services.AddSingleton<LocalAssetStore>();
            builder.Services.AddSingleton<IAssetStore>(s => s.GetRequiredService<LocalAssetStore>());

            builder.Services.AddSingleton<IDataExporter, LocalWorkerExporter>();
            builder.Services.AddSingleton<IRemoteAssetStore>(s => s.GetRequiredService<LocalAssetStore>());

            builder.Services.AddSingleton<Worker.Worker>();

            builder.Services.AddHostedService(s => s.GetRequiredService<Worker.Worker>());
            builder.Services.AddHostedService(s => s.GetRequiredService<LocalAssetStore>());
        }
        else
        {
            builder.Services.AddSingleton<IRemoteAssetStore, RemoteAssetStore>();
        }

        var app = builder.Build();

#if !DEBUG
        app.UseForwardedHeaders();
#else
        app.UseDeveloperExceptionPage();
#endif

        app.UseRouting();
        app.UseCors();

        app.MapControllers();

        using (var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
        {
            // if worker is enabled, run redis migrations
            if (scope.ServiceProvider.GetService<Worker.Worker>() != null)
            {
                await Worker.Program.ValidateRedisStructures(scope.ServiceProvider).ConfigureAwait(false);
            }
        }

        await app.RunAsync().ConfigureAwait(false);
    }
}

[JsonSerializable(typeof(ConnectionStatusResponse))]
[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower, WriteIndented = true)]
internal partial class ControllerSerializerContext : JsonSerializerContext;