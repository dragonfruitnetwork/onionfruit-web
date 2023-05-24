// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    public enum NetworkFlags : ushort
    {
        AnonymousProxy = (1 << 0), // A1
        SatelliteProvider = (1 << 1), // A2
        Anycast = (1 << 2), // A3
        Drop = (1 << 3) // XD
    }
}
