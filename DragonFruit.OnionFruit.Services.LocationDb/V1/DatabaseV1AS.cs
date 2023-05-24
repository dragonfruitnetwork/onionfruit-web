// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    internal class DatabaseV1AS : DatabaseV1Collection<DatabaseSourceAS, DatabaseAS>, IASDatabase, IBinarySearchable<DatabaseAS, uint>
    {
        public DatabaseV1AS(MemoryMappedViewAccessor view, IStringPool pool)
            : base(view, pool)
        {
        }

        IDatabaseAS IASDatabase.this[int index] => this[index];

        public IDatabaseAS GetAS(uint number)
        {
            return BinaryUtils.BinarySearch(this, number);
        }

        protected override DatabaseAS FromNative(DatabaseSourceAS source)
        {
            var asn = BinaryUtils.EnsureEndianness(source.number);
            return new DatabaseAS(asn, Pool[source.name_poolid]);
        }

        public IEnumerator<IDatabaseAS> GetEnumerator()
        {
            return ((IEnumerable<DatabaseSourceAS>)this).Select(FromNative).GetEnumerator();
        }
    }
}
