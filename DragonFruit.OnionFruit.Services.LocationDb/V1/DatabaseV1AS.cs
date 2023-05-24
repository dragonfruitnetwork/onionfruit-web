// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    [DebuggerDisplay("AS{Number} - {Name}")]
    internal record DatabaseV1ASEntry(uint Number, string Name) : IDatabaseAS, ISearchableItem
    {
        object ISearchableItem.Key => Number;

        public override string ToString() => $"AS{Number}";
    }

    internal class DatabaseV1AS : DatabaseV1Collection<DatabaseV1SourceAS, DatabaseV1ASEntry>, IASDatabase
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

        protected override DatabaseV1ASEntry FromNative(DatabaseV1SourceAS source)
        {
            var asn = BinaryUtils.EnsureEndianness(source.number);
            return new DatabaseV1ASEntry(asn, Pool[source.name_poolid]);
        }

        public IEnumerator<IDatabaseAS> GetEnumerator()
        {
            return ((IEnumerable<DatabaseV1SourceAS>)this).Select(FromNative).GetEnumerator();
        }
    }
}
