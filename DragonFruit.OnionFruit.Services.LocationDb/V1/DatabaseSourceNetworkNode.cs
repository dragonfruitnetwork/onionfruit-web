// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DatabaseSourceNetworkNode
    {
        // The node to checkout if the next bit in the series is zero
        public readonly uint zero;

        // The node to checkout if the next bit in the series is one
        public readonly uint one;

        public readonly uint network;
    }
}
