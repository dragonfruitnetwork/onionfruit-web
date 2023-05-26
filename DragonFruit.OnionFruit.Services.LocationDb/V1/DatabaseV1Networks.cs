// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO.MemoryMappedFiles;
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

        public IDatabaseNetwork this[uint index] => CreateWithPrefix(index, null);

        internal DatabaseNetwork CreateWithPrefix(uint index, NetworkPrefix prefix)
        {
            return FromSource(ElementAt(index), prefix);
        }

        private unsafe DatabaseNetwork FromSource(DatabaseSourceNetwork source, NetworkPrefix prefix)
        {
            var correctedAsn = BinaryUtils.EnsureEndianness(source.asn);
            var correctedFlags = BinaryUtils.EnsureEndianness(source.flags);
            var countryCode = Encoding.ASCII.GetString(source.country_code, 2);

            return new DatabaseNetwork(prefix, countryCode, correctedAsn, (NetworkFlags)correctedFlags);
        }
    }
}
