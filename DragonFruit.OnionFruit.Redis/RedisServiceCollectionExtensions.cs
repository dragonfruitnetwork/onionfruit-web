// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Redis.Assets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Redis.OM;
using Redis.OM.Contracts;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Redis;

public static class RedisServiceCollectionExtensions
{
    /// <summary>
    /// Registers the shared Redis connection and the asset index used to resolve the latest version of an asset.
    /// </summary>
    public static IServiceCollection AddOnionFruitRedis(this IServiceCollection services)
    {
        services.AddOptions<RedisOptions>()
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.TryAddSingleton<IConnectionMultiplexer>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<RedisOptions>>().Value;
            return ConnectionMultiplexer.Connect(ConfigurationOptions.Parse(options.ConnectionString));
        });

        services.TryAddSingleton<IRedisConnectionProvider>(provider => new RedisConnectionProvider(provider.GetRequiredService<IConnectionMultiplexer>()));
        services.TryAddSingleton<IAssetIndex, RedisAssetIndex>();

        return services;
    }
}
