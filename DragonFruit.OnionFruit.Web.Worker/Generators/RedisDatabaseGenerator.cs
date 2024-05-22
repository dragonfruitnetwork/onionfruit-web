// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Linq;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Database;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;
using Redis.OM.Contracts;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public class RedisDatabaseGenerator(IRedisConnectionProvider redis, OnionooDataSource torDirectory) : IDatabaseGenerator
{
    public async Task GenerateDatabase(IFileSink fileSink)
    {
        var table = redis.RedisCollection<OnionFruitNodeInfo>();
        var dbVersion = (long)torDirectory.DataLastModified.Subtract(DateTime.UnixEpoch).TotalSeconds;

        foreach (var relay in torDirectory.Relays)
        {
            // drop the port off the addresses
            var ipAddresses = relay.OrAddresses.Select(x => x[..x.LastIndexOf(':')]);

            // concat any exit addresses
            if (relay.ExitAddresses?.Length > 0)
                ipAddresses = ipAddresses.Concat(relay.ExitAddresses);

            var info = new OnionFruitNodeInfo
            {
                DatabaseVersion = dbVersion,
                Flags = relay.Flags,

                // onionoo returns country codes in lowercase, which need to be changed for compatibility with everything else.
                CountryCode = relay.CountryCode.ToUpperInvariant(),
                CountryName = CountryMap.Instance.GetCountryName(relay.CountryCode) ?? relay.CountryName ?? "Unknown Location",

                ProviderName = relay.ASName,
                ProviderNumber = int.TryParse(relay.ASN?[1..], out var asn) ? asn : null,
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