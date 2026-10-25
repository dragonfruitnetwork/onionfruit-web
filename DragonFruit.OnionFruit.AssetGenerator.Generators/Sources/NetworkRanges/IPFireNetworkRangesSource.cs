// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Globalization;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using DnsClient;
using DnsClient.Protocol;
using DragonFruit.Data;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using libloc;
using libloc.Abstractions;
using libloc.Access;
using SharpCompress.Compressors.Xz;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.NetworkRanges;

internal sealed class IPFireNetworkRangeMapSource(ILookupClient dnsClient, ApiClient apiClient) : ISource
{
    private const string LastUpdateRecordAddress = "_v1._db.location.ipfire.org";

    private static readonly IReadOnlySet<string> ProducedOutputNames = new HashSet<string>
    {
        SourceOutputNames.LocationDbIpv4,
        SourceOutputNames.LocationDbIpv6
    };

    public string Name => "location-db";

    public IReadOnlySet<string> OutputNames => ProducedOutputNames;
    public IReadOnlyDictionary<string, ISourceOutput> Outputs { get; private set; } = new Dictionary<string, ISourceOutput>();

    public async Task<bool> HasDataChangedAsync(DateTimeOffset lastVersionDate, CancellationToken ct)
    {
        var records = await dnsClient.QueryAsync(LastUpdateRecordAddress, QueryType.TXT, cancellationToken: ct);
        var date = records.Answers.OfType<TxtRecord>().SingleOrDefault()?.Text.First();

        if (string.IsNullOrEmpty(date))
        {
            return true;
        }

        return DateTimeOffset.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) > lastVersionDate;
    }

    public async Task FetchAsync(CancellationToken ct)
    {
        await using var dbFileStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

        await using (var downloadStream = await apiClient.PerformAsync<FileStream>(new LocationDbDownloadRequest(), ct))
        await using (var xzStream = new XZStream(downloadStream))
        {
            await xzStream.CopyToAsync(dbFileStream, ct);
        }

        dbFileStream.Position = 0;
        using var database = DatabaseLoader.LoadFromStream(dbFileStream);

        var ipv4Builder = new NetworkRangeMapBuilder<uint>();
        AddNetworks(ipv4Builder, database, AddressFamily.InterNetwork, AddressKeyConverter.ToUInt32);

        var ipv6Builder = new NetworkRangeMapBuilder<UInt128>();
        AddNetworks(ipv6Builder, database, AddressFamily.InterNetworkV6, AddressKeyConverter.ToUInt128);

        Outputs = new Dictionary<string, ISourceOutput>(2)
        {
            [SourceOutputNames.LocationDbIpv4] = ipv4Builder.Build(SourceOutputNames.LocationDbIpv4, AddressFamily.InterNetwork, database.CreatedAt, database.License),
            [SourceOutputNames.LocationDbIpv6] = ipv6Builder.Build(SourceOutputNames.LocationDbIpv6, AddressFamily.InterNetworkV6, database.CreatedAt, database.License),
        };
    }

    private static void AddNetworks<TAddress>(
        NetworkRangeMapBuilder<TAddress> builder,
        ILocationDatabase database,
        AddressFamily family,
        Func<IPAddress, TAddress> convert)
        where TAddress : IBinaryInteger<TAddress>
    {
        using var networks = database.GetEnumerator(family, flattened: false);
        while (networks.MoveNext())
        {
            var network = networks.Current;
            if (network == null)
            {
                continue;
            }

            builder.AddRange(convert(network.Network.First), convert(network.Network.Last), network.CountryCode);
        }
    }
}