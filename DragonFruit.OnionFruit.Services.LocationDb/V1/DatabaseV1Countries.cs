using System.Collections.Generic;
using System.Diagnostics;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    [DebuggerDisplay("{Code} - {Name}")]
    internal record DatabaseV1Country(string Code, string ContinentCode, string Name) : IDatabaseCountry, ISearchableItem
    {
        object ISearchableItem.Key => Code;
    }

    internal unsafe class DatabaseV1Countries : DatabaseV1Collection<DatabaseV1SourceCountry, DatabaseV1Country>, ICountryDatabase, IEnumerable<IDatabaseCountry>
    {
        public DatabaseV1Countries(MemoryMappedViewAccessor view, IStringPool pool)
            : base(view, pool)
        {
        }

        IDatabaseCountry ICountryDatabase.this[int index] => this[index];

        public IDatabaseCountry GetCountry(string code) => BinaryUtils.BinarySearch(this, code);

        protected override DatabaseV1Country FromNative(DatabaseV1SourceCountry source)
        {
            var code = Encoding.ASCII.GetString(source.code, 2);
            var continent = Encoding.ASCII.GetString(source.continent_code, 2);

            return new DatabaseV1Country(code, continent, Pool[source.name_poolid]);
        }

        public IEnumerator<IDatabaseCountry> GetEnumerator() => ((IEnumerable<DatabaseV1SourceCountry>)this).Select(FromNative).GetEnumerator();
    }
}
