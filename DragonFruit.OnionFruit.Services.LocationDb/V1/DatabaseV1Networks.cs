// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO.MemoryMappedFiles;
using System.Text;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    internal unsafe class DatabaseV1Networks : DatabaseV1Collection<DatabaseSourceNetwork, IDatabaseNetwork>, INetworkDatabase
    {
        public DatabaseV1Networks(MemoryMappedViewAccessor view, IStringPool pool)
            : base(view, pool)
        {
        }

        protected override DatabaseNetwork FromNative(DatabaseSourceNetwork source)
        {
            var correctedAsn = BinaryUtils.EnsureEndianness(source.asn);
            var correctedFlags = BinaryUtils.EnsureEndianness(source.flags);
            var countryCode = Encoding.ASCII.GetString(source.country_code, 2);

            return new DatabaseNetwork(countryCode, correctedAsn, (NetworkFlags)correctedFlags);
        }
    }
}
