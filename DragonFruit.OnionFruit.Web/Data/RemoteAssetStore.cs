// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Configuration;
using DragonFruit.OnionFruit.Web.Worker.Configuration;
using DragonFruit.OnionFruit.Web.Worker.Storage;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Data;

public class RemoteAssetStore(IConnectionMultiplexer redis, IOptions<ServerOptions> serverOptions, IOptions<RedisOptions> redisOptions) : IRemoteAssetStore
{
    public async Task<AssetInfo> GetAssetInfo(string fileName)
    {
        var db = redis.GetDatabase();
        var latestAssetRevision = await db.HashGetAsync($"{redisOptions.Value.KeyPrefix}:{S3StorageManager.VersionedAssetMapName}", fileName);

        if (!latestAssetRevision.HasValue)
        {
            return null;
        }

        var version = latestAssetRevision.ToString();
        var assetVersion = long.TryParse(version, out var timestamp) ? DateTimeOffset.FromUnixTimeSeconds(timestamp) : DateTimeOffset.UtcNow;

        return new AssetInfo(fileName, string.Format(serverOptions.Value.RemoteAssetPublicUrl, $"{version}/{fileName}"), assetVersion, version);
    }
}
