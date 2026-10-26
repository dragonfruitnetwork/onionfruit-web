// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.IO.Compression;
using System.Numerics;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Networks;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing.Models;
using DragonFruit.OnionFruit.Redis.Nodes;
using Google.Protobuf;
using RangeMap;
using OnionDbMessage = OnionDb;
using OnionDbCountryMessage = OnionDbCountry;
using AddressRangeMessage = AddressRange;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.OnionDb;

public class OnionDbGenerator : IGenerator
{
    internal const string DatabaseOutputKey = "onion.db";
    internal const string CompressedDatabaseOutputKey = "onion.db.br";

    public virtual string Name => "onion-db";

    public IReadOnlySet<string> Sources { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        SourceOutputNames.LocationDbIpv4,
        SourceOutputNames.LocationDbIpv6,
        SourceOutputNames.OnionooRelays
    });

    public virtual IReadOnlySet<string> Produces { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        DatabaseOutputKey,
        CompressedDatabaseOutputKey
    });

    public Task GenerateAsync(GenerationContext context, CancellationToken ct)
    {
        var database = CreateBaseDb(context);

        var relays = context.GetOutput<ITorRelayStatusOutput>(SourceOutputNames.OnionooRelays);
        var relaysByCountry = relays.Relays
            .Where(relay => !string.IsNullOrWhiteSpace(relay.CountryCode))
            .GroupBy(relay => relay.CountryCode, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(group => group.Key, group => (IReadOnlyList<TorRelayDetails>)group.ToList(), StringComparer.OrdinalIgnoreCase);

        var ipv4 = context.TryGetOutput<INetworkRangeDatabase<uint>>(SourceOutputNames.LocationDbIpv4, out var v4Database) ? v4Database : null;
        var ipv6 = context.TryGetOutput<INetworkRangeDatabase<UInt128>>(SourceOutputNames.LocationDbIpv6, out var v6Database) ? v6Database : null;

        // use the countries present in the location databases where possible, otherwise fall back to
        // the countries reported by Onionoo (this allows the lite database to run without location data).
        var countryCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (ipv4 != null || ipv6 != null)
        {
            if (ipv4 != null)
            {
                countryCodes.UnionWith(ipv4.Keys);
            }

            if (ipv6 != null)
            {
                countryCodes.UnionWith(ipv6.Keys);
            }
        }
        else
        {
            countryCodes.UnionWith(relaysByCountry.Keys);
        }

        foreach (var countryCode in countryCodes.OrderBy(code => code, StringComparer.OrdinalIgnoreCase))
        {
            ct.ThrowIfCancellationRequested();

            if (!relaysByCountry.TryGetValue(countryCode, out var nodes))
            {
                continue;
            }

            var countryData = new OnionDbCountryMessage
            {
                CountryCode = countryCode,
                CountryName = nodes.Select(relay => relay.CountryName).FirstOrDefault(name => !string.IsNullOrWhiteSpace(name)) ?? string.Empty
            };

            // calculate total node counts
            foreach (var nodeFlag in nodes.SelectMany(node => node.RawFlags))
            {
                switch (nodeFlag)
                {
                    case nameof(TorNodeFlags.Exit):
                        countryData.ExitNodeCount++;
                        break;

                    case nameof(TorNodeFlags.Guard):
                        countryData.EntryNodeCount++;
                        break;

                    case nameof(TorNodeFlags.Fast):
                        countryData.FastNodeCount++;
                        break;

                    case nameof(TorNodeFlags.Running):
                        countryData.OnlineNodeCount++;
                        break;
                }

                countryData.TotalNodeCount++;
            }

            WriteCountryAddressRanges(countryData, GetRanges(ipv4, countryCode), GetRanges(ipv6, countryCode));
            database.Countries.Add(countryData);
        }

        return OnDatabaseGeneratedAsync(context, database, ct);
    }

    protected virtual OnionDbMessage CreateBaseDb()
    {
        return new OnionDbMessage
        {
            MagicBytes = ByteString.CopyFrom(0xDB, 0x01),

            TorLicense = "CC0",
            DbVersion = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    protected virtual OnionDbMessage CreateBaseDb(GenerationContext context)
    {
        var database = CreateBaseDb();
        var relays = context.GetOutput<ITorRelayStatusOutput>(SourceOutputNames.OnionooRelays);

        database.TorDirVersion = relays.Published.ToUnixTimeSeconds();

        var ipv4 = context.TryGetOutput<INetworkRangeDatabase<uint>>(SourceOutputNames.LocationDbIpv4, out var v4) ? v4 : null;
        var ipv6 = context.TryGetOutput<INetworkRangeDatabase<UInt128>>(SourceOutputNames.LocationDbIpv6, out var v6) ? v6 : null;

        var createdAt = ipv4?.CreatedAt ?? ipv6?.CreatedAt;
        if (createdAt.HasValue)
        {
            database.GeoDirVersion = createdAt.Value.ToUnixTimeSeconds();
        }

        var license = ipv4?.License ?? ipv6?.License;
        if (!string.IsNullOrWhiteSpace(license))
        {
            database.GeoLicense = license;
        }

        return database;
    }

    protected virtual void WriteCountryAddressRanges(OnionDbCountryMessage country, IEnumerable<RangeInclusive<uint>> v4Ranges, IEnumerable<RangeInclusive<UInt128>> v6Ranges)
    {
        country.V4Ranges.AddRange(v4Ranges.Select(range => CreateAddressRange(range.Start, range.End)));
        country.V6Ranges.AddRange(v6Ranges.Select(range => CreateAddressRange(range.Start, range.End)));
    }

    protected virtual async Task OnDatabaseGeneratedAsync(GenerationContext context, OnionDbMessage database, CancellationToken ct)
    {
        await using var dbStream = await context.Workspace.CreateFileAsync(DatabaseOutputKey, ct);

        database.WriteTo(dbStream);
        await dbStream.FlushAsync(ct);

        // write brotli compressed version as well
        dbStream.Seek(0, SeekOrigin.Begin);

        await using var compressionStream = new BrotliStream(await context.Workspace.CreateFileAsync(CompressedDatabaseOutputKey, ct), CompressionLevel.SmallestSize, leaveOpen: false);
        await dbStream.CopyToAsync(compressionStream, ct);
    }

    private static IReadOnlyList<RangeInclusive<TAddress>> GetRanges<TAddress>(INetworkRangeDatabase<TAddress>? database, string countryCode)
        where TAddress : IBinaryInteger<TAddress>
    {
        if (database == null)
        {
            return [];
        }

        if (database.TryGetValue(countryCode, out var ranges))
        {
            return ranges;
        }

        // the underlying databases may use a different casing for country codes
        foreach (var key in database.Keys)
        {
            if (string.Equals(key, countryCode, StringComparison.OrdinalIgnoreCase) && database.TryGetValue(key, out ranges))
            {
                return ranges;
            }
        }

        return [];
    }

    private static AddressRangeMessage CreateAddressRange(uint start, uint end)
    {
        Span<byte> startBytes = stackalloc byte[4];
        Span<byte> endBytes = stackalloc byte[4];

        BinaryPrimitives.WriteUInt32BigEndian(startBytes, start);
        BinaryPrimitives.WriteUInt32BigEndian(endBytes, end);

        return new AddressRangeMessage
        {
            Start = ByteString.CopyFrom(startBytes),
            End = ByteString.CopyFrom(endBytes)
        };
    }

    private static AddressRangeMessage CreateAddressRange(UInt128 start, UInt128 end)
    {
        Span<byte> startBytes = stackalloc byte[16];
        Span<byte> endBytes = stackalloc byte[16];

        BinaryPrimitives.WriteUInt128BigEndian(startBytes, start);
        BinaryPrimitives.WriteUInt128BigEndian(endBytes, end);

        return new AddressRangeMessage
        {
            Start = ByteString.CopyFrom(startBytes),
            End = ByteString.CopyFrom(endBytes)
        };
    }
}