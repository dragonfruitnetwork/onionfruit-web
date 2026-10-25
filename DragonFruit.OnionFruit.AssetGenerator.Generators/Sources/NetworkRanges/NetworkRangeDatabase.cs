using System.Collections;
using System.Net.Sockets;
using System.Numerics;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Networks;
using RangeMap;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.NetworkRanges;

internal sealed class NetworkRangeDatabase<TAddress> : INetworkRangeDatabase<TAddress> where TAddress : IBinaryInteger<TAddress>
{
    private readonly RangeInclusiveMap<TAddress, int> _ranges;
    private readonly IReadOnlyList<string> _countries;

    internal NetworkRangeDatabase(string name, AddressFamily family, RangeInclusiveMap<TAddress, int> ranges, IReadOnlyList<string> countries, DateTimeOffset? createdAt, string? license)
    {
        Name = name;
        AddressFamily = family;
        _ranges = ranges;
        _countries = countries;
        CreatedAt = createdAt;
        License = license;
    }

    public string Name { get; }

    public AddressFamily AddressFamily { get; }

    public int RangeCount => _ranges.Count;

    public int CountryCount => Grouped.Count;

    public DateTimeOffset? CreatedAt { get; }

    public string? License { get; }

    public IReadOnlyList<RangeInclusive<TAddress>> this[string key] => Grouped[key];

    public IEnumerable<string> Keys => Grouped.Keys;

    public IEnumerable<IReadOnlyList<RangeInclusive<TAddress>>> Values => Grouped.Values;

    public int Count => Grouped.Count;

    public bool ContainsKey(string key) => Grouped.ContainsKey(key);

    public bool TryGetValue(string key, out IReadOnlyList<RangeInclusive<TAddress>> value) => Grouped.TryGetValue(key, out value!);

    public IEnumerator<KeyValuePair<string, IReadOnlyList<RangeInclusive<TAddress>>>> GetEnumerator() => Grouped.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    private IReadOnlyDictionary<string, IReadOnlyList<RangeInclusive<TAddress>>> Grouped => field ??= BuildGrouped();

    private IReadOnlyDictionary<string, IReadOnlyList<RangeInclusive<TAddress>>> BuildGrouped()
    {
        var groups = new Dictionary<string, List<RangeInclusive<TAddress>>>();

        foreach (var (range, countryIndex) in _ranges)
        {
            var country = _countries[countryIndex];
            if (!groups.TryGetValue(country, out var list))
            {
                list = new List<RangeInclusive<TAddress>>();
                groups.Add(country, list);
            }

            list.Add(range);
        }

        return groups.ToDictionary(g => g.Key, g => (IReadOnlyList<RangeInclusive<TAddress>>)g.Value);
    }
}
