// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;

namespace DragonFruit.OnionFruit.Services.LocationDb.Abstractions
{
    public interface ILocationDatabase : IDisposable
    {
        int Version { get; }

        string Vendor { get; }
        string License { get; }
        string Description { get; }

        IASDatabase AS { get; }
        ICountryDatabase Countries { get; }
    }
}
