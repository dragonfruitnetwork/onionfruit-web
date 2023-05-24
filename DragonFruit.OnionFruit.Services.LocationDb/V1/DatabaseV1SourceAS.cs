// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct DatabaseV1SourceAS
    {
        // The AS number
        public readonly uint number;

        // Name
        public readonly uint name_poolid;
    }
}
