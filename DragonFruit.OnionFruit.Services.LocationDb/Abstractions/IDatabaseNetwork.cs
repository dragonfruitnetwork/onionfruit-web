// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.OnionFruit.Services.LocationDb.V1;

namespace DragonFruit.OnionFruit.Services.LocationDb.Abstractions
{
    public interface IDatabaseNetwork
    {
        string CountryCode { get; }

        uint ASN { get; }

        NetworkFlags Flags { get; }
    }
}
