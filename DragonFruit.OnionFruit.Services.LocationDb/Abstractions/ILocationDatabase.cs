// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace DragonFruit.OnionFruit.Services.LocationDb.Abstractions
{
    public interface ILocationDatabase : IEnumerable<IAddressLocatedNetwork>, IDisposable
    {
        int Version { get; }

        string Vendor { get; }
        string License { get; }
        string Description { get; }

        DateTime CreatedAt { get; }

        IASDatabase AS { get; }
        INetworkDatabase Networks { get; }
        ICountryDatabase Countries { get; }

        IAddressLocatedNetwork ResolveAddress(IPAddress address);
        IEnumerator<IAddressLocatedNetwork> GetEnumerator(AddressFamily family);
    }
}
