// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Countries;

[Flags]
public enum RelayCapabilities
{
    None = 0,
    Guard = 1 << 0,
    Exit = 1 << 1
}