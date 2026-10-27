// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Globalization;
using Amazon.S3;
using Amazon.S3.Model;
using Coravel.Invocable;
using DragonFruit.OnionFruit.AssetGenerator.Publishing;
using DragonFruit.OnionFruit.Redis.Assets;
using Microsoft.Extensions.Options;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Services;

public sealed class AssetExpiryPurgeJob(
    IAmazonS3 client,
    IOptions<S3AssetPublisherOptions> publisherOptions,
    IAssetIndex assetIndex,
    ILogger<AssetExpiryPurgeJob> logger) : IInvocable
{
    private const int ListPageSize = 1000;
    private const int MaxKeysPerDeleteRequest = 1000;

    public async Task Invoke()
    {
        var options = publisherOptions.Value;

        var cutoff = DateTimeOffset.UtcNow.AddDays(-options.ExpireOldAssetsAfter);
        var currentVersions = await assetIndex.GetAllVersionsAsync();

        var expiredKeys = await FindExpiredKeysAsync(options, currentVersions, cutoff);
        if (expiredKeys.Count == 0)
        {
            logger.LogDebug("Expired-asset purge completed: no eligible objects found");
            return;
        }

        logger.LogInformation("Removing {count} expired asset version(s)", expiredKeys.Count);
        await RemoveAsync(options, expiredKeys);
    }

    private async Task<List<string>> FindExpiredKeysAsync(S3AssetPublisherOptions options, IReadOnlyDictionary<string, string> currentVersions, DateTimeOffset cutoff)
    {
        var expiredKeys = new List<string>();
        var listPrefix = BuildListPrefix(options.KeyPrefix);
        string? continuationToken = null;

        do
        {
            var page = await client.ListObjectsV2Async(new ListObjectsV2Request
            {
                BucketName = options.BucketName,
                Prefix = listPrefix,
                ContinuationToken = continuationToken,
                MaxKeys = ListPageSize
            });

            foreach (var obj in page.S3Objects ?? [])
            {
                if (!TryParseAssetKey(options.KeyPrefix, obj.Key, out var version, out var asset))
                {
                    continue;
                }

                if (currentVersions.TryGetValue(asset, out var currentVersion) && currentVersion == version)
                {
                    continue;
                }

                if (obj.LastModified < cutoff.UtcDateTime)
                {
                    expiredKeys.Add(obj.Key);
                }
            }

            continuationToken = page.NextContinuationToken;
        } while (!string.IsNullOrEmpty(continuationToken));

        return expiredKeys;
    }

    private async Task RemoveAsync(S3AssetPublisherOptions options, IReadOnlyList<string> keys)
    {
        foreach (var batch in keys.Chunk(MaxKeysPerDeleteRequest))
        {
            await client.DeleteObjectsAsync(new DeleteObjectsRequest
            {
                BucketName = options.BucketName,
                Objects = batch.Select(key => new KeyVersion {Key = key}).ToList()
            });
        }
    }

    private static string? BuildListPrefix(string? keyPrefix)
    {
        return string.IsNullOrWhiteSpace(keyPrefix)
            ? null
            : keyPrefix.Trim('/') + "/";
    }

    private static bool TryParseAssetKey(string? keyPrefix, string key, out string version, out string asset)
    {
        version = string.Empty;
        asset = string.Empty;

        var relative = key.TrimStart('/');

        if (!string.IsNullOrWhiteSpace(keyPrefix))
        {
            var prefix = keyPrefix.Trim('/');

            if (!relative.StartsWith(prefix + "/", StringComparison.Ordinal))
            {
                return false;
            }

            relative = relative[(prefix.Length + 1)..];
        }

        var separator = relative.IndexOf('/');
        if (separator <= 0 || separator >= relative.Length - 1)
        {
            return false;
        }

        version = relative[..separator];
        asset = relative[(separator + 1)..];

        return long.TryParse(version, NumberStyles.None, CultureInfo.InvariantCulture, out _) && AssetPath.IsValid(asset);
    }
}
