// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Immutable;
using System.Collections.ObjectModel;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing;
using DragonFruit.OnionFruit.Web.Shared.Redis.Nodes;
using Redis.OM.Contracts;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.RedisDb;

public class RedisDatabaseGenerator(IRedisConnectionProvider redis) : IGenerator
{
    public string Name => "redis-db";

    public IReadOnlySet<string> Sources { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        SourceOutputNames.OnionooRelays
    });

    public IReadOnlySet<string> Produces => ImmutableHashSet<string>.Empty;

    public async Task GenerateAsync(GenerationContext context, CancellationToken ct)
    {
        var relays = context.GetOutput<ITorRelayStatusOutput>(SourceOutputNames.OnionooRelays);
        var table = redis.RedisCollection<OnionFruitNodeInfo>();
        var databaseVersion = relays.Published.ToUnixTimeSeconds();

        foreach (var relay in relays.Relays)
        {
            ct.ThrowIfCancellationRequested();

            // drop the port off the addresses. exit addresses don't include a port.
            var ipAddresses = (relay.OrAddresses ?? []).Select(RemovePort);

            if (relay.ExitAddresses?.Length > 0)
            {
                ipAddresses = ipAddresses.Concat(relay.ExitAddresses);
            }

            var info = new OnionFruitNodeInfo
            {
                IpAddress = string.Empty,
                DatabaseVersion = databaseVersion,
                Flags = relay.Flags,

                // onionoo returns country codes in lowercase, which need to be changed for compatibility with everything else.
                CountryCode = relay.CountryCode?.ToUpperInvariant(),
                CountryName = relay.CountryName ?? "Unknown Location",

                ProviderName = relay.ASName,
                ProviderNumber = ParseProviderNumber(relay.ASN)
            };

            foreach (var address in ipAddresses)
            {
                // create a copy to prevent change trackers doing anything odd
                var addressInfo = info.Clone();
                addressInfo.IpAddress = address;

                await table.InsertAsync(addressInfo);
            }
        }

        // the source lists every node, so anything left on an older version is no longer present
        foreach (var node in Queryable.Where(table, node => node.DatabaseVersion < databaseVersion))
        {
            ct.ThrowIfCancellationRequested();
            await table.DeleteAsync(node);
        }
    }

    private static string RemovePort(string address)
    {
        var portSeparator = address.LastIndexOf(':');
        return portSeparator >= 0 ? address[..portSeparator] : address;
    }

    private static int? ParseProviderNumber(string? asn)
    {
        if (string.IsNullOrWhiteSpace(asn))
        {
            return null;
        }

        var number = asn.StartsWith("AS", StringComparison.OrdinalIgnoreCase) ? asn[2..] : asn;
        return int.TryParse(number, out var providerNumber) ? providerNumber : null;
    }
}