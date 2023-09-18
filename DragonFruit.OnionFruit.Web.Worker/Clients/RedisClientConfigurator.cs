using System;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker.Clients;

public static class RedisClientConfigurator
{
    /// <summary>
    /// Creates a <see cref="IConnectionMultiplexer"/> from the provided <see cref="IConfiguration"/>.
    /// Further applying socket-related settings based on whether it is in worker mode.
    /// </summary>
    public static IConnectionMultiplexer CreateConnectionMultiplexer(IConfiguration config, bool workerMode)
    {
        var connectionString = config["Redis:ConnectionString"];
        ConfigurationOptions redisConfig;

        if (!string.IsNullOrEmpty(connectionString))
        {
            redisConfig = ConfigurationOptions.Parse(connectionString);
        }
        else
        {
            redisConfig = new ConfigurationOptions
            {
                User = config["Redis:User"],
                Password = config["Redis:Pass"],
                Ssl = config["Redis:Ssl"]?.Equals("true", StringComparison.InvariantCultureIgnoreCase) == true,
                EndPoints =
                {
                    {
                        config["Redis:Host"] ?? "localhost", int.TryParse(config["Redis:Port"], out var p) ? p : 6379
                    }
                }
            };
        }

        if (workerMode)
        {
            // worker mode doesn't need massive throughput as it isn't handling user-requests
            redisConfig.SocketManager = new SocketManager(workerCount: 2, options: SocketManager.SocketManagerOptions.UseThreadPool);
        }

        return ConnectionMultiplexer.Connect(redisConfig);
    }
}