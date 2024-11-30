// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using Microsoft.Extensions.Configuration;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Data
{
    public class RemoteAssetStore(IConnectionMultiplexer redis, IConfiguration config) : IRemoteAssetStore
    {
        public async Task<AssetInfo> GetAssetInfo(string fileName)
        {
            var redisPrefix = config[RedisClientConfigurator.PrefixConfigKey] ?? RedisClientConfigurator.DefaultKeyPrefix;

            var db = redis.GetDatabase();
            var latestAssetRevision = await db.HashGetAsync($"{redisPrefix}:{S3StorageManager.VersionedAssetMapName}", fileName);

            if (!latestAssetRevision.HasValue)
            {
                return null;
            }

            var version = latestAssetRevision.ToString();
            var assetVersion = long.TryParse(version, out var timestamp) ? DateTimeOffset.FromUnixTimeSeconds(timestamp) : DateTimeOffset.UtcNow;

            return new AssetInfo(fileName, string.Format(config["Server:RemoteAssetPublicUrl"], $"{version}/{fileName}"), assetVersion, version);
        }
    }
}