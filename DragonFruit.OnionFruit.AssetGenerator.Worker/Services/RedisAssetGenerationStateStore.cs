// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Worker.Configuration;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Services;

public sealed class RedisAssetGenerationStateStore(IConnectionMultiplexer redis, IOptions<RedisStateStoreOptions> options) : IAssetGenerationStateStore
{
    public async Task<DateTimeOffset?> GetLastRunAsync(string sourceName, CancellationToken ct)
    {
        var value = await redis.GetDatabase().StringGetAsync(GetKey(sourceName));

        return value.HasValue && long.TryParse(value.ToString(), out var timestamp)
            ? DateTimeOffset.FromUnixTimeSeconds(timestamp)
            : null;
    }

    public Task SetLastRunAsync(string sourceName, DateTimeOffset timestamp, CancellationToken ct)
    {
        return redis.GetDatabase().StringSetAsync(GetKey(sourceName), timestamp.ToUnixTimeSeconds());
    }

    private RedisKey GetKey(string sourceName) => $"{options.Value.KeyPrefix}:source:{sourceName}:last-run";
}