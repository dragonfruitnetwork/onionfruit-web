using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Web.Worker.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(ConfigureServices)
                .Build();

            await host.RunAsync().ConfigureAwait(false);
        }

        private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
        {
            // redis + redis.om
            services.AddSingleton<IConnectionMultiplexer>(RedisClientConfigurator.CreateConnectionMultiplexer(context.Configuration, true));
            services.AddSingleton<IRedisConnectionProvider>(s => new RedisConnectionProvider(s.GetRequiredService<IConnectionMultiplexer>()));
            
            // api
            services.AddSingleton<ApiClient, WorkerClient>();

            // timed service
            services.AddHostedService<Worker>();
        }
    }
}