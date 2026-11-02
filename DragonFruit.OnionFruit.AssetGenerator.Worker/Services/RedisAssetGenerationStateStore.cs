// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Abstractions;
using DragonFruit.OnionFruit.Redis;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Services;

public sealed class RedisAssetGenerationStateStore(IConnectionMultiplexer redis, IOptions<RedisOptions> options) : IAssetGenerationStateStore
{
    public async Task<DateTimeOffset?> GetLastRunAsync(string sourceName, CancellationToken ct)
    {
        var value = await redis.GetDatabase().StringGetAsync(GetKey(sourceName)).WaitAsync(ct);

        return value.HasValue && long.TryParse(value.ToString(), out var timestamp)
            ? DateTimeOffset.FromUnixTimeSeconds(timestamp)
            : null;
    }

    public Task SetLastRunAsync(string sourceName, DateTimeOffset timestamp, CancellationToken ct)
    {
        return redis.GetDatabase().StringSetAsync(GetKey(sourceName), timestamp.ToUnixTimeSeconds()).WaitAsync(ct);
    }

    private RedisKey GetKey(string sourceName) => RedisKeys.Create(options.Value, "source", sourceName, "last-run");
}
