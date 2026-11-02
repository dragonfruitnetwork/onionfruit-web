// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Net.Sockets;
using System.Numerics;
using RangeMap;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.NetworkRanges;

internal sealed class NetworkRangeMapBuilder<TAddress> where TAddress : IBinaryInteger<TAddress>
{
    private readonly List<string> _countries = new();
    private readonly Dictionary<string, int> _countryIndexes = new();
    private readonly RangeInclusiveMap<TAddress, int> _ranges = new();

    public int RangeCount => _ranges.Count;
    public int CountryCount => _countries.Count;

    public void AddRange(RangeInclusive<TAddress> range, string countryCode)
    {
        ArgumentException.ThrowIfNullOrEmpty(countryCode);

        _ranges.Insert(range, GetOrAddCountryIndex(countryCode));
    }

    /// <summary>
    /// Adds the inclusive range <c>start..=end</c> and attributes it to a country.
    /// </summary>
    public void AddRange(TAddress start, TAddress end, string countryCode) => AddRange(new RangeInclusive<TAddress>(start, end), countryCode);

    /// <summary>
    /// Produces a snapshot of the ranges added until the point-of-calling.
    /// </summary>
    public NetworkRangeDatabase<TAddress> Build(string outputName, AddressFamily family, DateTimeOffset? createdAt = null, string? license = null)
    {
        return new NetworkRangeDatabase<TAddress>(outputName, family, _ranges.Clone(), _countries.ToArray(), createdAt, license);
    }

    private int GetOrAddCountryIndex(string countryCode)
    {
        if (_countryIndexes.TryGetValue(countryCode, out var index))
        {
            return index;
        }

        index = _countries.Count;

        _countries.Add(countryCode);
        _countryIndexes.Add(countryCode, index);

        return index;
    }
}