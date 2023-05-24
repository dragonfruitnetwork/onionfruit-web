// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    public record DatabaseNetwork(string CountryCode, uint ASN, NetworkFlags Flags) : IDatabaseNetwork;
}
