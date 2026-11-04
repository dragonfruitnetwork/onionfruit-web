// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Reflection;
using DragonFruit.OnionFruit.AssetGenerator.Worker.Configuration;
using DragonFruit.OnionFruit.Web.Shared.Redis.Nodes;
using Microsoft.Extensions.Options;
using Redis.OM;
using Redis.OM.Contracts;
using Redis.OM.Modeling;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Services;

public static class RedisIndexInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        var options = services.GetRequiredService<IOptions<WorkerOptions>>().Value;

        if (!options.CreateIndexes && !options.RegenIndexes)
        {
            return;
        }

        var redis = services.GetRequiredService<IRedisConnectionProvider>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(RedisIndexInitializer));

        foreach (var type in typeof(OnionFruitNodeInfo).Assembly.ExportedTypes.Where(type => type.GetCustomAttribute<DocumentAttribute>() != null))
        {
            ct.ThrowIfCancellationRequested();

            if (options.RegenIndexes)
            {
                logger.LogInformation("Dropping Redis index for {type}", type.Name);
                await redis.Connection.DropIndexAsync(type);
            }

            logger.LogInformation("Creating Redis index for {type} (if required)", type.Name);
            await redis.Connection.CreateIndexAsync(type);
        }
    }
}