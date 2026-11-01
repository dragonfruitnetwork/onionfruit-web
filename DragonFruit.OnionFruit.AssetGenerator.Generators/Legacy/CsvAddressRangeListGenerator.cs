// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Buffers.Binary;
using System.Collections.ObjectModel;
using System.Net;
using System.Numerics;
using System.Text;
using CsvHelper;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Networks;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Legacy;

internal sealed class CsvAddressRangeListGenerator : IGenerator
{
    private const string Ipv4OutputKey = "legacy/geoip";
    private const string Ipv6OutputKey = "legacy/geoip6";

    public string Name => "legacy/geoip";

    public IReadOnlySet<string> Sources { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        SourceOutputNames.LocationDbIpv4,
        SourceOutputNames.LocationDbIpv6,
    });

    public IReadOnlySet<string> Produces { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        Ipv4OutputKey,
        Ipv6OutputKey,
    });

    public async Task GenerateAsync(GenerationContext context, CancellationToken ct)
    {
        var ipv4 = context.GetOutput<INetworkRangeDatabase<uint>>(SourceOutputNames.LocationDbIpv4);
        await WriteRangesCsvAsync(context.Workspace, Ipv4OutputKey, ipv4, address => address.ToString(), CreateComment("IPv4"), ct);

        var ipv6 = context.GetOutput<INetworkRangeDatabase<UInt128>>(SourceOutputNames.LocationDbIpv6);
        await WriteRangesCsvAsync(context.Workspace, Ipv6OutputKey, ipv6, address =>
        {
            Span<byte> bytes = stackalloc byte[16];
            BinaryPrimitives.WriteUInt128BigEndian(bytes, address);

            return new IPAddress(bytes).ToString();
        }, CreateComment("IPv6"), ct);
    }

    private static string CreateComment(string addressFamily) => $"# OnionFruit GeoIP File ({addressFamily}). Generated using IPFire's location.db licenced under CC BY-SA 4.0.";

    private static async Task WriteRangesCsvAsync<TAddress>(
        IWorkspace workspace,
        string key,
        INetworkRangeDatabase<TAddress> database,
        Func<TAddress, string> formatAddress,
        string comment,
        CancellationToken ct)
        where TAddress : IBinaryInteger<TAddress>
    {
        await using var file = await workspace.CreateFileAsync(key, ct);
        await using var textWriter = new StreamWriter(file, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), 16 * 1024, leaveOpen: true);

        await textWriter.WriteAsync($"{comment}\r\n".AsMemory(), ct);

        await using (var csv = CsvWriter.Create(textWriter, leaveOpen: true))
        {
            foreach (var (country, ranges) in database.OrderBy(x => x.Key, StringComparer.Ordinal))
            {
                if (string.IsNullOrWhiteSpace(country) || !country.All(char.IsAsciiLetter))
                {
                    continue;
                }

                foreach (var range in ranges.OrderBy(r => r.Start))
                {
                    ct.ThrowIfCancellationRequested();

                    csv.WriteField(formatAddress(range.Start));
                    csv.WriteField(formatAddress(range.End));
                    csv.WriteField(country);

                    await csv.EndRecordAsync(ct);
                }
            }
        }

        await textWriter.FlushAsync(ct);
    }
}