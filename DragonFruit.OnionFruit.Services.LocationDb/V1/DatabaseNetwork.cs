// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    public record DatabaseNetwork(IPAddress FirstAddress, IPAddress LastAddress, int PrefixLength, string CountryCode, uint ASN, NetworkFlags Flags) : IAddressLocatedNetwork;
}
