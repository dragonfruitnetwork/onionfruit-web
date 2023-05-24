// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Runtime.InteropServices;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    /// <summary>
    /// The v1 location.db header format.
    /// Based on the format.h header file (https://git.ipfire.org/?p=location/libloc.git;a=blob;f=src/libloc/format.h;h=030394bc72e4003f7d1def2f8d50176cab4201b4;hb=HEAD)
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct DatabaseV1SourceHeader
    {
        private const int SignatureMaxLength = 2048;

        // UNIX timestamp when the database was created
        public ulong created_at;

        // Vendor who created the database
        public uint vendor;

        // Description of the database
        public uint description;

        // License of the database
        public uint license;

        // Tells us where the ASes start
        public uint as_offset;
        public uint as_length;

        // Tells us where the networks start
        public uint network_data_offset;
        public uint network_data_length;

        // Tells us where the network nodes start
        public uint network_tree_offset;
        public uint network_tree_length;

        // Tells us where the countries start
        public uint countries_offset;
        public uint countries_length;

        // Tells us where the pool starts
        public uint pool_offset;
        public uint pool_length;

        // Signatures
        public ushort signature1_length;
        public ushort signature2_length;

        public fixed byte signature1[SignatureMaxLength];

        public fixed byte signature2[SignatureMaxLength];

        // Add some padding for future extensions
        public fixed byte padding[32];
    }
}
