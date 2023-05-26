// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Net;

namespace DragonFruit.OnionFruit.Services.LocationDb.Abstractions
{
    public interface IAddressLocatedNetwork : IDatabaseNetwork
    {
        IPAddress FirstAddress { get; }
        IPAddress LastAddress { get; }

        int PrefixLength { get; }
    }
}
