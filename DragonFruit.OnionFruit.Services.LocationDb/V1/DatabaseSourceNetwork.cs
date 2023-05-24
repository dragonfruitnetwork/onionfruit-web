// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DatabaseSourceNetwork
    {
        // The start address and prefix will be encoded in the tree

        // The country this network is located in
        public fixed byte country_code[2];

        // ASN\\t
        public readonly uint asn;

        // Flags
        public readonly ushort flags;

        // Reserved
        private fixed char padding[2];
    }
}
