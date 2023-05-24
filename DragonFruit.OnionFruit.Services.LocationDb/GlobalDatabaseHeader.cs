// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Services.LocationDb
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct GlobalDatabaseHeader
    {
        internal const int MagicByteLength = 7;

        public fixed byte magic[MagicByteLength];
        public readonly byte version;
    }
}
