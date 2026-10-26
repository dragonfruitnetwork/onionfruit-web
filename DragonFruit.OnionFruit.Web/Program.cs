// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.Data;
using DragonFruit.Data.Serializers;
using DragonFruit.OnionFruit.Redis;
using DragonFruit.OnionFruit.Web.Configuration;
using DragonFruit.OnionFruit.Web.Services;
using libloc.Access;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.HttpOverrides;

namespace DragonFruit.OnionFruit.Web;

public static class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Logging
            .ClearProviders()
            .AddSimpleConsole(o =>
            {
                o.SingleLine = true;
                o.IncludeScopes = false;
                o.TimestampFormat = "[dd/MM/yyyy hh:mm:ss] ";
            });

        builder.Services.AddOptions<RedisOptions>()
            .Bind(builder.Configuration.GetSection(RedisOptions.SectionName));

        builder.Services.AddOptions<AssetDeliveryOptions>()
            .Bind(builder.Configuration.GetSection(AssetDeliveryOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        builder.Services.AddMemoryCache(options => options.SizeLimit = 10_000);
        builder.Services.AddOnionFruitRedis();

        builder.Services.AddSingleton<ApiClient, ApiClient<ApiJsonSerializer>>();
        builder.Services.AddLocationDb();

        builder.Services.AddSingleton<IAssetRedirectService, AssetRedirectService>();
        builder.Services.AddSingleton<IConnectionStatusService, ConnectionStatusService>();

        builder.Services.AddControllers();
        builder.Services.AddProblemDetails();
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

        builder.Services.AddHttpLogging(o =>
        {
            o.CombineLogs = true;
            o.LoggingFields = HttpLoggingFields.RequestMethod | HttpLoggingFields.RequestPath | HttpLoggingFields.ResponseStatusCode | HttpLoggingFields.Duration;
        });

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedForHeaderName = "CF-Connecting-IP";
            options.ForwardedProtoHeaderName = "X-Forwarded-Proto";
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
        });

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseHttpLogging();
        app.UseForwardedHeaders();
        app.UseCors();
        app.MapControllers();

        await app.RunAsync();
    }
}
