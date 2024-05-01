// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
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

// ReSharper disable InconsistentNaming
namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public record NetworkAddressRangeInfo(IPAddressRange Network, string CountryCode);

public class LocationDbSource(ILookupClient dnsClient, ApiClient apiClient) : IDataSource, IDisposable
{
    private const string LastUpdateRecordAddress = "_v1._db.location.ipfire.org";

    public ILocationDatabase Database { get; private set; }

    public IReadOnlyList<NetworkAddressRangeInfo> IPv4AddressRanges { get; private set; }
    public IReadOnlyList<NetworkAddressRangeInfo> IPv6AddressRanges { get; private set; }

    public ILookup<string, IPAddressRange> IPv4CountryAddressRanges { get; private set; }
    public ILookup<string, IPAddressRange> IPv6CountryAddressRanges { get; private set; }

    public async Task<bool> HasDataChanged(DateTimeOffset lastVersionDate)
    {
        var records = await dnsClient.QueryAsync(LastUpdateRecordAddress, QueryType.TXT).ConfigureAwait(false);
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

        await using (var downloadStream = await apiClient.PerformAsync<FileStream>(new LocationDbDownloadRequest()).ConfigureAwait(false))
        await using (var xzStream = new XZStream(downloadStream))
        {
            await xzStream.CopyToAsync(dbFileStream).ConfigureAwait(false);
        }

        Database = DatabaseLoader.LoadFromStream(dbFileStream);
        NetworkSortResult networkSortResult;
        IntPtr networkArray = IntPtr.Zero;

        try
        {
            var networkCount = BuildUnmanagedNetworkStructArray(Database, out networkArray);
            NativeMethods.PerformNetworkSort(networkArray, networkCount, out networkSortResult);
        }
        finally
        {
            // clear up unmanaged network array at earliest opportunity
            if (networkArray != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(networkArray);
            }
        }

        try
        {
            // run processors in parallel
            var v4AddressProcess = Task.Factory.StartNew(() => GetIPv4AddressRanges(networkSortResult.v4Entries, networkSortResult.v4Count), TaskCreationOptions.LongRunning);
            var v6AddressProcess = Task.Factory.StartNew(() => GetIPv6AddressRanges(networkSortResult.v6Entries, networkSortResult.v6Count), TaskCreationOptions.LongRunning);

            IPv4AddressRanges = await v4AddressProcess.ConfigureAwait(false);
            IPv6AddressRanges = await v6AddressProcess.ConfigureAwait(false);

            IPv4CountryAddressRanges = IPv4AddressRanges.ToLookup(x => x.CountryCode, x => x.Network, StringComparer.OrdinalIgnoreCase);
            IPv6CountryAddressRanges = IPv6AddressRanges.ToLookup(x => x.CountryCode, x => x.Network, StringComparer.OrdinalIgnoreCase);
        }
        finally
        {
            // ensure unmanaged memory is cleared regardless of success/failure
            NativeMethods.ClearSortResult(ref networkSortResult);
        }
    }

    /// <summary>
    /// Copies the network tree from the provided <see cref="ILocationDatabase"/> to a block of unmanaged memory
    /// consisting of <see cref="NetworkEntry"/> structs and returns the pointer to the start of the block.
    /// </summary>
    /// <remarks>
    /// A call to <see cref="Marshal.FreeHGlobal"/> is required to deallocate the unmanaged memory block (<see cref="arrayPtr"/>)
    /// </remarks>
    /// <param name="networks">The network database to get the tree from</param>
    /// <param name="arrayPtr">(Output) pointer to the start of the unmanaged block</param>
    /// <returns>The number of <see cref="NetworkEntry"/> structs written</returns>
    private static unsafe int BuildUnmanagedNetworkStructArray(ILocationDatabase networks, out IntPtr arrayPtr)
    {
        var unmanagedNetworkList = Marshal.AllocHGlobal(Unsafe.SizeOf<NetworkEntry>() * networks.Networks.Count);
        var asciiCache = new Dictionary<string, byte[]>(networks.Countries.Count);
        var currentItem = (NetworkEntry*)unmanagedNetworkList;
        var count = 0;

        try
        {
            foreach (var network in networks)
            {
                // use asciiCache to prevent loads of pointless array allocations
                if (!asciiCache.TryGetValue(network.CountryCode, out var asciiBuffer))
                {
                    var buffer = Encoding.ASCII.GetBytes(network.CountryCode, 0, 2);

                    asciiCache[network.CountryCode] = buffer;
                    asciiBuffer = buffer;
                }

                // get pointer and increment for next iteration (if one)
                var currentEntry = currentItem++;

                fixed (byte* networkBytes = network.Network.Network.MapToIPv6().GetAddressBytes())
                {
                    Buffer.MemoryCopy(networkBytes, currentEntry->network, 16, 16);
                }

                fixed (byte* countryCodeBytes = asciiBuffer)
                {
                    Buffer.MemoryCopy(countryCodeBytes, currentEntry->country_code, 2, 2);
                }

                currentEntry->cidr = network.Network.Cidr;
                count++;
            }

            arrayPtr = unmanagedNetworkList;
            return count;
        }
        catch
        {
            Marshal.FreeHGlobal(unmanagedNetworkList);
            arrayPtr = IntPtr.Zero;

            throw;
        }
    }

    private static unsafe NetworkAddressRangeInfo[] GetIPv4AddressRanges(IntPtr start, nint length)
    {
        Span<byte> v4AddressBytes = stackalloc byte[4];
        var v4NetworkRanges = new NetworkAddressRangeInfo[length];

        for (var i = 0; i < length; i++)
        {
            var entry = *(IPv4NetworkRange*)(start + sizeof(IPv4NetworkRange) * i);

            var startAddress = ParseAddress(v4AddressBytes, entry.start_address);
            var endAddress = ParseAddress(v4AddressBytes, entry.end_address);

            v4NetworkRanges[i] = new NetworkAddressRangeInfo(new IPAddressRange(startAddress, endAddress), Encoding.ASCII.GetString(entry.country_code, 2));
        }

        return v4NetworkRanges;
    }

    private static unsafe NetworkAddressRangeInfo[] GetIPv6AddressRanges(IntPtr start, nint length)
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
    /// Converts a <c>byte*</c> to an <see cref="IPAddress"/> using the <see cref="buffer"/> size as the size of the <see cref="addressPtr"/>
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