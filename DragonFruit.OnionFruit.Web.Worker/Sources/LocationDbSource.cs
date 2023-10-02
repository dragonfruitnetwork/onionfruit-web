using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using DnsClient;
using DragonFruit.Data;
using libloc;
using libloc.Abstractions;
using libloc.Access;
using SharpCompress.Compressors.Xz;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

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

    public async Task<bool> HasDataChanged(DateTimeOffset lastVersionDate)
    {
        var records = await _dnsClient.QueryAsync(LastUpdateRecordAddress, QueryType.TXT).ConfigureAwait(false);
        var date = records.Answers.SingleOrDefault()?.ToString();

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

        var addressRanges = Database
            .AsParallel()
            .OrderBy(x => x.Network.Cidr)
            .Select(dbEntry =>
            {
                // addresses pulled from the database are always ipv4-mapped-to-ipv6
                var startValue = dbEntry.Network.FirstUsable.GetAddressBytes();
                var endValue = dbEntry.Network.LastUsable.GetAddressBytes();

                // addresses are big-endian (i.e. if little endian, the order needs switching)
                if (BitConverter.IsLittleEndian)
                {
                    Array.Reverse(startValue);
                    Array.Reverse(endValue);
                }

                // no UInt128 method exists in BitConverter (for now)
                var startValueStruct = Unsafe.ReadUnaligned<UInt128>(ref MemoryMarshal.GetReference(startValue.AsSpan()));
                var endValueStruct = Unsafe.ReadUnaligned<UInt128>(ref MemoryMarshal.GetReference(endValue.AsSpan()));

                return new NumericalNetworkEntry(new NumericalRange<UInt128>(startValueStruct, endValueStruct), dbEntry);
            })
            .GroupBy(x => x.Entry.Network.Network.IsIPv4MappedToIPv6)
            .Select(x =>
            {
                // create a copy as it'll be mutated
                var items = x.ToList();
                var rangeBuffer = new List<NumericalRange<UInt128>>(2);

                // The range consolation uses the algorithm from https://softwareengineering.stackexchange.com/a/241386
                // as it would have been counterproductive to re-implement the RangeInclusiveMap from the rust crate
                // and it's better than building a native library to handle the logic + interop, etc.

                var i = 0;
                while (i < items.Count)
                {
                    foreach (var superior in items.Skip(i + 1))
                    {
                        superior.Range.SubtractFrom(items[i].Range, rangeBuffer);

                        // If span is completely covered, remove from list and compensate for the removal.
                        if (!rangeBuffer.Any())
                        {
                            items.RemoveAt(i);
                            i--;

                            break;
                        }
                        
                        // update range with first value
                        items[i] = items[i] with { Range = rangeBuffer.First() };
                        
                        // if there was a second value, insert a new one with the new range
                        if (rangeBuffer.Count > 1)
                        {
                            items.Insert(i + 1, items[i] with { Range = rangeBuffer.Last() });
                        }
                    }

                    i++;
                }

                return items;
            })
            .ToList();
    }

    public void Dispose()
    {
        Database?.Dispose();
    }
}

internal record NumericalNetworkEntry(NumericalRange<UInt128> Range, IAddressLocatedNetwork Entry);

internal readonly struct NumericalRange<T> where T : INumber<T>
{
    public NumericalRange(T start, T end)
    {
        Start = start;
        End = end;
    }

    public T Start { get; }
    public T End { get; }

    public void SubtractFrom(NumericalRange<T> other, List<NumericalRange<T>> outputBuffer)
    {
        outputBuffer.Clear();

        if (Start > other.End || other.Start > End)
        {
            outputBuffer.Add(other);
            return;
        }

        if (Start > other.Start)
        {
            outputBuffer.Add(new NumericalRange<T>(other.Start, Start));
        }

        if (End < other.End)
        {
            outputBuffer.Add(new NumericalRange<T>(End, other.End));
        }
    }
}