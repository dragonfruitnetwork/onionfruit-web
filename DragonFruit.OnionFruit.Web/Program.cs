using System;
using System.Text.Json;
using System.Threading.Tasks;
using DnsClient;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Web.Worker;
using libloc.Access;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpOverrides;
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

        Worker.Program.ConfigureLogging(builder.Logging, builder.Configuration, "Server");

        builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower);
        builder.Services.AddCors(cors => cors.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyMethod();
            policy.AllowAnyHeader();
            policy.SetPreflightMaxAge(TimeSpan.FromHours(12));

            policy.SetIsOriginAllowed(s =>
            {
                var uri = new Uri(s, UriKind.Absolute);
                return uri.Host == "localhost" || uri.Host.EndsWith("dragonfruit.network");
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

        // register worker if needed
        if (builder.Configuration["Worker:Enable"]?.Equals("true", StringComparison.OrdinalIgnoreCase) == true)
        {
            builder.Services.AddHostedService<Worker.Worker>();
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

        await app.RunAsync().ConfigureAwait(false);
    }
}