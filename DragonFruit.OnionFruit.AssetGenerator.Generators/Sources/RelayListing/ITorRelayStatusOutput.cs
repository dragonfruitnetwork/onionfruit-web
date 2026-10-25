// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing.Models;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing;

public interface ITorRelayStatusOutput : ISourceOutput
{
    DateTimeOffset Published { get; }

    IReadOnlyList<TorRelayDetails> Relays { get; }
}