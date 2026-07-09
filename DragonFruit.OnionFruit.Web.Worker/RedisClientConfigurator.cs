// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Worker.Configuration;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker;

public static class RedisClientConfigurator
{
    /// <summary>
    /// Creates a <see cref="IConnectionMultiplexer"/> from the provided <see cref="RedisOptions"/>.
    /// Further applying socket-related settings based on whether it is in worker mode.
    /// </summary>
    public static IConnectionMultiplexer CreateConnectionMultiplexer(RedisOptions options, bool workerMode)
    {
        ConfigurationOptions redisConfig;

        if (!string.IsNullOrEmpty(options.ConnectionString))
        {
            redisConfig = ConfigurationOptions.Parse(options.ConnectionString);
        }
        else
        {
            redisConfig = new ConfigurationOptions
            {
                User = options.User,
                Password = options.Pass,
                Ssl = options.Ssl,
                EndPoints =
                {
                    {
                        options.Host, options.Port
                    }
                }
            };
        }

        if (workerMode)
        {
            // worker mode doesn't need massive throughput as it isn't handling user-requests
            redisConfig.SocketManager = new SocketManager(workerCount: 2, options: SocketManager.SocketManagerOptions.UseThreadPool);
        }

        if (options.DisableCertValidation)
        {
            redisConfig.CertificateValidation += delegate { return true; };
        }

        return ConnectionMultiplexer.Connect(redisConfig);
    }
}
