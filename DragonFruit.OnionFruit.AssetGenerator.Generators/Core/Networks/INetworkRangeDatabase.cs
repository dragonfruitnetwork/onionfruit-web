// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Net.Sockets;
using System.Numerics;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using RangeMap;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Networks;

public interface INetworkRangeDatabase<TAddress> : INetworkRangeDatabase, IReadOnlyDictionary<string, IReadOnlyList<RangeInclusive<TAddress>>> where TAddress : IBinaryInteger<TAddress>;

public interface INetworkRangeDatabase : ISourceOutput
{
    AddressFamily AddressFamily { get; }

    int RangeCount { get; }
    int CountryCount { get; }

    DateTimeOffset? CreatedAt => null;
    string? License => null;
}