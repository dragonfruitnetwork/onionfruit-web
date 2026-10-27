// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Redis.Assets;

public sealed class RedisAssetIndex(IConnectionMultiplexer redis, IOptions<RedisOptions> options) : IAssetIndex
{
    private RedisKey IndexKey => RedisKeys.Create(options.Value, "assets");

    public async Task<string?> GetLatestVersionAsync(string assetPath, CancellationToken ct = default)
    {
        var path = AssetPath.Normalize(assetPath);
        if (!AssetPath.IsValid(path))
        {
            return null;
        }

        var value = await redis.GetDatabase().HashGetAsync(IndexKey, path).WaitAsync(ct);

        return value.HasValue ? value.ToString() : null;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetAllVersionsAsync(CancellationToken ct = default)
    {
        var entries = await redis.GetDatabase().HashGetAllAsync(IndexKey).WaitAsync(ct);

        return entries.ToDictionary(entry => entry.Name.ToString(), entry => entry.Value.ToString(), StringComparer.Ordinal);
    }

    public Task SetLatestVersionAsync(string assetPath, string version, CancellationToken ct = default)
    {
        var path = AssetPath.Normalize(assetPath);

        return AssetPath.IsValid(path) && !string.IsNullOrWhiteSpace(version)
            ? redis.GetDatabase().HashSetAsync(IndexKey, path, version).WaitAsync(ct)
            : Task.CompletedTask;
    }

    public async Task SetLatestVersionsAsync(IEnumerable<KeyValuePair<string, string>> versions, CancellationToken ct = default)
    {
        var entries = versions
            .Select(pair => new KeyValuePair<string, string>(AssetPath.Normalize(pair.Key), pair.Value))
            .Where(pair => AssetPath.IsValid(pair.Key) && !string.IsNullOrWhiteSpace(pair.Value))
            .GroupBy(pair => pair.Key, StringComparer.Ordinal)
            .Select(group => new HashEntry(group.Key, group.Last().Value))
            .ToArray();

        if (entries.Length == 0)
        {
            return;
        }

        await redis.GetDatabase().HashSetAsync(IndexKey, entries).WaitAsync(ct);
    }
}
