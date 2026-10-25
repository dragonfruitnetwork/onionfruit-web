// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Countries;

public interface ICountryCapabilities : ISourceOutput, IReadOnlyDictionary<string, RelayCapabilities>;