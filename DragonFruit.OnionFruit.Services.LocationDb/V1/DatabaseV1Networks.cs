// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Net;
using System.Text;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    internal class DatabaseV1Networks : DatabaseV1Collection<DatabaseSourceNetwork>, INetworkDatabase
    {
        public DatabaseV1Networks(MemoryMappedViewAccessor view)
            : base(view)
        {
        }

        public IDatabaseNetwork this[uint index] => FromSource(ElementAt(index), IPAddress.None, IPAddress.None, 0);

        internal DatabaseNetwork CreateWithPrefix(uint index, Span<byte> v6AddressBytes, int prefix)
        {
            Debug.Assert(v6AddressBytes.Length == 16);

            Span<byte> bitmask = stackalloc byte[v6AddressBytes.Length];
            Span<byte> firstAddressBytes = stackalloc byte[v6AddressBytes.Length];
            Span<byte> lastAddressBytes = stackalloc byte[v6AddressBytes.Length];

            for (int i = prefix, j = 0; i > 0; i -= 8, j++)
            {
                bitmask[j] = i >= 8 ? (byte)0xff : (byte)(0xff << (8 - i));
            }

            for (int i = 0; i < v6AddressBytes.Length; i++)
            {
                // perform AND to get first address
                firstAddressBytes[i] = (byte)(v6AddressBytes[i] & bitmask[i]);

                // perform OR to get last address
                lastAddressBytes[i] = (byte)(v6AddressBytes[i] | ~bitmask[i]);
            }

            var networkInfo = ElementAt(index);
            var firstAddress = new IPAddress(firstAddressBytes);
            var lastAddress = new IPAddress(lastAddressBytes);

            return FromSource(networkInfo, firstAddress, lastAddress, prefix);
        }

        private unsafe DatabaseNetwork FromSource(DatabaseSourceNetwork source, IPAddress firstAddress, IPAddress lastAddress, int prefixLength)
        {
            var correctedAsn = BinaryUtils.EnsureEndianness(source.asn);
            var correctedFlags = BinaryUtils.EnsureEndianness(source.flags);
            var countryCode = Encoding.ASCII.GetString(source.country_code, 2);

            return new DatabaseNetwork(firstAddress, lastAddress, prefixLength, countryCode, correctedAsn, (NetworkFlags)correctedFlags);
        }
    }
}
