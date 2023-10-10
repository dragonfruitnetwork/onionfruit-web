using System;
using System.Buffers;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using DnsClient;
using DnsClient.Protocol;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Web.Worker.Native;
using libloc;
using libloc.Abstractions;
using libloc.Access;
using NetTools;
using SharpCompress.Compressors.Xz;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public record NetworkAddressRangeInfo(IPAddressRange Network, string CountryCode);

public class LocationDbSource : IDataSource, IDisposable
{
    private readonly ILookupClient _dnsClient;
    private readonly ApiClient _apiClient;

    private const string LastUpdateRecordAddress = "_v1._db.location.ipfire.org";

    public LocationDbSource(ILookupClient dnsClient, ApiClient apiClient)
    {
        _dnsClient = dnsClient;
        _apiClient = apiClient;
    }

    public ILocationDatabase Database { get; private set; }
    
    public IReadOnlyList<NetworkAddressRangeInfo> IPv4AddressRanges { get; private set; }
    public IReadOnlyList<NetworkAddressRangeInfo> IPv6AddressRanges { get; private set; }

    public ILookup<string, IPAddressRange> IPv4CountryAddressRanges { get; private set; }
    public ILookup<string, IPAddressRange> IPv6CountryAddressRanges { get; private set; }
    
    public async Task<bool> HasDataChanged(DateTimeOffset lastVersionDate)
    {
        var records = await _dnsClient.QueryAsync(LastUpdateRecordAddress, QueryType.TXT).ConfigureAwait(false);
        var date = records.Answers.OfType<TxtRecord>().SingleOrDefault()?.Text.First();
        
        if (string.IsNullOrEmpty(date))
        {
            return true;
        }

        return DateTimeOffset.Parse(date, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind) > lastVersionDate;
    }

    public async Task CollectData()
    {
        var dbFileStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

        await using (var downloadStream = await _apiClient.PerformAsync<FileStream>(new LocationDbDownloadRequest()).ConfigureAwait(false))
        await using (var xzStream = new XZStream(downloadStream))
        {
            await xzStream.CopyToAsync(dbFileStream).ConfigureAwait(false);
        }

        Database = DatabaseLoader.LoadFromStream(dbFileStream);

        // start with a relatively large buffer to pass all entries into
        var networkList = new List<NetworkEntry>(1500000);
        var asciiCache = new Dictionary<string, byte[]>(Database.Countries.Count);

        try
        {
            foreach (var network in Database)
            {
                var entry = new NetworkEntry
                {
                    network = network.Network.Network.GetAddressBytes(),
                    cidr = network.Network.Cidr
                };

                // use asciiCache to prevent loads of pointless array allocations
                if (!asciiCache.TryGetValue(network.CountryCode, out var asciiBuffer))
                {
                    var buffer = ArrayPool<byte>.Shared.Rent(2);
                    Encoding.ASCII.GetBytes(network.CountryCode, 0, 2, buffer, 0);

                    asciiCache[network.CountryCode] = buffer;
                    asciiBuffer = buffer;
                }

                entry.country_code = asciiBuffer;
                networkList.Add(entry);
            }

            NativeMethods.PerformNetworkSort(networkList.ToArray(), networkList.Count, out var networkSortResult);

            try
            {
                IPv4AddressRanges = GetIPv4AddressRanges(networkSortResult.v4Entries, networkSortResult.v4Count);
                IPv6AddressRanges = GetIPv6AddressRanges(networkSortResult.v6Entries, networkSortResult.v6Count);

                IPv4CountryAddressRanges = IPv4AddressRanges.ToLookup(x => x.CountryCode, x => x.Network, StringComparer.OrdinalIgnoreCase);
                IPv6CountryAddressRanges = IPv6AddressRanges.ToLookup(x => x.CountryCode, x => x.Network, StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
                // ensure unmanaged memory is cleared regardless of success/failure
                NativeMethods.ClearSortResult(ref networkSortResult);
            }
        }
        finally
        {
            // return all used buffers
            foreach (var arr in asciiCache.Values)
            {
                ArrayPool<byte>.Shared.Return(arr);
            }
        
            asciiCache.Clear();
            networkList.Clear();
        }
    }

    private unsafe NetworkAddressRangeInfo[] GetIPv4AddressRanges(IntPtr start, nint length)
    {
        var v4NetworkRanges = new NetworkAddressRangeInfo[length];
        Span<byte> v4AddressBytes = stackalloc byte[4];
        
        // collect all data and process into something useful
        for (var i = 0; i < length; i++)
        {
            var entry = *(IPv4NetworkRange*)(start + sizeof(IPv4NetworkRange) * i);

            var startAddress = ParseAddress(v4AddressBytes, entry.start_address);
            var endAddress = ParseAddress(v4AddressBytes, entry.end_address);
            
            v4NetworkRanges[i] = new NetworkAddressRangeInfo(new IPAddressRange(startAddress, endAddress), Encoding.ASCII.GetString(entry.country_code, 2));
        }

        return v4NetworkRanges;
    }

    private unsafe NetworkAddressRangeInfo[] GetIPv6AddressRanges(IntPtr start, nint length)
    {
        var v6NetworkRanges = new NetworkAddressRangeInfo[length];
        Span<byte> v6AddressBytes = stackalloc byte[16];
        
        for (var i = 0; i < length; i++)
        {
            var entry = *(IPv6NetworkRange*)(start + sizeof(IPv6NetworkRange) * i);

            var startAddress = ParseAddress(v6AddressBytes, entry.start_address);
            var endAddress = ParseAddress(v6AddressBytes, entry.end_address);
            
            v6NetworkRanges[i] = new NetworkAddressRangeInfo(new IPAddressRange(startAddress, endAddress), Encoding.ASCII.GetString(entry.country_code, 2));
        }

        return v6NetworkRanges;
    }

    /// <summary>
    /// Converts a byte* to a <see cref="IPAddress"/>, using the <see cref="buffer"/> size as the size of the <see cref="addressPtr"/>
    /// </summary>
    private static unsafe IPAddress ParseAddress(Span<byte> buffer, byte* addressPtr)
    {
        fixed (byte* bufferPtr = buffer)
        {
            Buffer.MemoryCopy(addressPtr, bufferPtr, buffer.Length, buffer.Length);
        }

        return new IPAddress(buffer);
    }

    public void Dispose()
    {
        Database?.Dispose();
    }
}