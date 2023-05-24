using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    internal unsafe class DatabaseV1Countries : DatabaseV1Collection<DatabaseSourceCountry, DatabaseCountry>, ICountryDatabase, IBinarySearchable<DatabaseCountry, string>
    {
        public DatabaseV1Countries(MemoryMappedViewAccessor view, IStringPool pool)
            : base(view, pool)
        {
        }

        IDatabaseCountry ICountryDatabase.this[int index] => this[index];

        public IDatabaseCountry GetCountry(string code)
        {
            return BinaryUtils.BinarySearch(this, code);
        }

        protected override DatabaseCountry FromNative(DatabaseSourceCountry source)
        {
            var code = Encoding.ASCII.GetString(source.code, 2);
            var continent = Encoding.ASCII.GetString(source.continent_code, 2);

            return new DatabaseCountry(code, continent, Pool[source.name_poolid]);
        }

        public IEnumerator<IDatabaseCountry> GetEnumerator()
        {
            return ((IEnumerable<DatabaseSourceCountry>)this).Select(FromNative).GetEnumerator();
        }
    }
}
