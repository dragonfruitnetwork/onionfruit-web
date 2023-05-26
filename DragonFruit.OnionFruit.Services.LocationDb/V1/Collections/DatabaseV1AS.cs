// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;
using DragonFruit.OnionFruit.Services.LocationDb.V1.Source;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1.Collections
{
    internal class DatabaseV1AS : DatabaseV1Collection<DatabaseSourceAS>, IASDatabase
    {
        private readonly IStringPool _pool;

        public DatabaseV1AS(MemoryMappedViewAccessor view, IStringPool pool)
            : base(view)
        {
            _pool = pool;
        }

        public IDatabaseAS this[uint asn] => BinaryUtils.BinarySearch(Count, x => FromSource(ElementAt(x)), asn);

        private DatabaseAS FromSource(DatabaseSourceAS source)
        {
            var asn = BinaryUtils.EnsureEndianness(source.number);
            return new DatabaseAS(asn, _pool[source.name_poolid]);
        }

        public IEnumerator<IDatabaseAS> GetEnumerator()
        {
            return ((IEnumerable<DatabaseSourceAS>)this).Select(FromSource).GetEnumerator();
        }
    }
}
