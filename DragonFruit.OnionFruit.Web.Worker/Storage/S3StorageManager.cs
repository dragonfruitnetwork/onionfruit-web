// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public class S3StorageManager(IConfiguration configuration, IConnectionMultiplexer redis, AmazonS3Client client, string bucketName) : IDataExporter, IHostedService
{
    public const string VersionedAssetMapName = "versioned-assets";
    private const string ExpiryQueueName = "versioned-assets-expiry-queue";

    private Timer _purgeCheckTimer;

    /// <summary>
    /// Expiry time, in days, to request removal of old assets
    /// </summary>
    public double ExpireOldAssetsAfter { get; set; } = int.TryParse(configuration["S3:ExpireOldAssetsAfter"], out var val) ? val : 30;

    public Task PerformUpload(IUploadFileSource source)
    {
        var version = source.Version;
        var database = redis?.GetDatabase();

        var keyPrefix = configuration[RedisClientConfigurator.PrefixConfigKey] ?? RedisClientConfigurator.DefaultKeyPrefix;
        var assetMapKey = new RedisKey($"{keyPrefix}:{VersionedAssetMapName}");
        var expiryQueueKey = new RedisKey($"{keyPrefix}:{ExpiryQueueName}");

        return source.IterateAllStreams((key, stream) => PerformItemUpload(key, version, stream, database, assetMapKey, expiryQueueKey));
    }

    private async Task PerformItemUpload(string assetKey, string version, FileStream stream, IDatabaseAsync database, RedisKey versionedAssetMapKey, RedisKey expiryQueueKey)
    {
        stream.Seek(0, SeekOrigin.Begin);

        var checksum = await SHA256.HashDataAsync(stream);
        var request = new PutObjectRequest
        {
            ChecksumAlgorithm = ChecksumAlgorithm.SHA256,
            ChecksumSHA256 = Convert.ToBase64String(checksum),
            DisablePayloadSigning = true,
            AutoCloseStream = false,
            BucketName = bucketName,
            InputStream = stream,
            Key = $"{version}/{assetKey}"
        };

        stream.Seek(0, SeekOrigin.Begin);
        await client.PutObjectAsync(request).ConfigureAwait(false);

        if (database == null)
        {
            return;
        }

        // update version map, add old one to expiry queue
        var oldVersion = await database.HashGetAsync(versionedAssetMapKey, assetKey).ConfigureAwait(false);

        await database.HashSetAsync(versionedAssetMapKey, assetKey, version).ConfigureAwait(false);

        if (oldVersion.HasValue)
        {
            await database.SortedSetAddAsync(expiryQueueKey, $"{oldVersion.ToString()}/{assetKey}", DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
    }

    private async Task PerformPurgeCheck()
    {
        var keyPrefix = configuration[RedisClientConfigurator.PrefixConfigKey] ?? RedisClientConfigurator.DefaultKeyPrefix;
        var maxRemovalAge = DateTimeOffset.UtcNow.AddDays(-ExpireOldAssetsAfter).ToUnixTimeSeconds();
        var queueKey = new RedisKey($"{keyPrefix}:{ExpiryQueueName}");
        var database = redis.GetDatabase();

        var markedAssets = await database.SortedSetRangeByScoreWithScoresAsync(queueKey, stop: maxRemovalAge).ConfigureAwait(false);
        if (markedAssets.Length == 0)
        {
            return;
        }

        var removalRequest = new DeleteObjectsRequest
        {
            BucketName = bucketName,
            Objects = markedAssets.Select(x => new KeyVersion
            {
                Key = x.Element.ToString()
            }).ToList()
        };

        await client.DeleteObjectsAsync(removalRequest).ConfigureAwait(false);
        await database.SortedSetRemoveRangeByScoreAsync(queueKey, 0, maxRemovalAge).ConfigureAwait(false);
    }

    Task IHostedService.StartAsync(CancellationToken cancellationToken)
    {
        if (redis == null)
        {
            return Task.CompletedTask;
        }

        _purgeCheckTimer?.Dispose();
        _purgeCheckTimer = new Timer(s => _ = PerformPurgeCheck(), null, TimeSpan.Zero, TimeSpan.FromHours(24));

        return Task.CompletedTask;
    }

    Task IHostedService.StopAsync(CancellationToken cancellationToken)
    {
        _purgeCheckTimer?.Dispose();
        _purgeCheckTimer = null;

        return Task.CompletedTask;
    }
}