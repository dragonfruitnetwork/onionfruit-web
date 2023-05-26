// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1.Source
{
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DatabaseSourceCountry
    {
        public fixed byte code[2];

        public fixed byte continent_code[2];

        public uint name_poolid;
    }
}
