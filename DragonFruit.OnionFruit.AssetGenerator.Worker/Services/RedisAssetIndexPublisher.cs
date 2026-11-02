// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Abstractions;
using DragonFruit.OnionFruit.Redis.Assets;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Services;

/// <summary>
/// Updates the Redis asset index after the assets have been published to their storage backend.
/// </summary>
public sealed class RedisAssetIndexPublisher(IAssetIndex assetIndex, ILogger<RedisAssetIndexPublisher> logger) : IAssetPublisher
{
    public async Task PublishAsync(AssetPublishContext context, CancellationToken ct)
    {
        var versions = context.Assets.ToDictionary(asset => asset, _ => context.Version, StringComparer.Ordinal);

        await assetIndex.SetLatestVersionsAsync(versions, ct);

        logger.LogInformation("Updated asset index with {count} asset(s) at version {version}", versions.Count, context.Version);
    }
}
