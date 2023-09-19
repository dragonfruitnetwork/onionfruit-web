using System;
using System.Linq;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Database;
using Redis.OM.Contracts;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public class OnionFruitRedisNodeDatabaseGenerator : IDatabaseGenerator
{
    private readonly IRedisConnectionProvider _redis;
    private readonly TorDirectoryInfoSource _torDirectory;

    public OnionFruitRedisNodeDatabaseGenerator(IRedisConnectionProvider redis, TorDirectoryInfoSource torDirectory)
    {
        _redis = redis;
        _torDirectory = torDirectory;
    }

    public async Task GenerateDatabase()
    {
        var table = _redis.RedisCollection<OnionFruitNodeInfo>();
        var dbVersion = (long)_torDirectory.DataLastModified.Subtract(DateTime.UnixEpoch).TotalSeconds;
        
        foreach (var relay in _torDirectory.Relays)
        {
            // drop the port off the addresses
            var ipAddresses = relay.OrAddresses.Select(x => x[..x.LastIndexOf(':')]);

            // concat any exit addresses
            if (relay.ExitAddresses?.Any() == true)
                ipAddresses = ipAddresses.Concat(relay.ExitAddresses);

            var info = new OnionFruitNodeInfo
            {
                DatabaseVersion = dbVersion,

                Flags = relay.Flags,
                ProviderName = relay.ASName,
                CountryCode = relay.CountryCode,
                CountryName = relay.CountryName ?? "Unknown Location"
            };

            // write info to redis
            foreach (var address in ipAddresses)
            {
                // create a copy to prevent change trackers doing anything odd
                var addressInfo = info.Clone();
                addressInfo.IpAddress = address;

                await table.InsertAsync(addressInfo).ConfigureAwait(false);
            }
        }

        foreach (var node in table.Where(x => x.DatabaseVersion > dbVersion))
        {
            await table.DeleteAsync(node).ConfigureAwait(false);
        }
    }
}