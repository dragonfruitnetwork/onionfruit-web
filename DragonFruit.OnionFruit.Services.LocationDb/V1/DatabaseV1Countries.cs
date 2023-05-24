using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    internal unsafe class DatabaseV1Countries : ICountryDatabase, IBinarySearchable<DatabaseV1Country>, IEnumerable<DatabaseV1SourceCountry>, IDisposable
    {
        private readonly IStringPool _pool;
        private readonly MemoryMappedViewAccessor _countryView;

        internal DatabaseV1Countries(IStringPool pool, MemoryMappedViewAccessor countryView)
        {
            _pool = pool;
            _countryView = countryView;

            Count = (int)_countryView.Capacity / sizeof(DatabaseV1SourceCountry);
        }

        public int Count { get; }

        public IDatabaseCountry this[int index] => GetInternal(index);
        public IDatabaseCountry this[string code] => BinaryUtils.BinarySearch(this, code);

        ISearchableItem IBinarySearchable<DatabaseV1Country>.this[int index] => GetInternal(index);

        private DatabaseV1Country GetInternal(int index)
        {
            _countryView.Read(index * sizeof(DatabaseV1SourceCountry), out DatabaseV1SourceCountry native);
            return MapNative(native);
        }

        private DatabaseV1Country MapNative(DatabaseV1SourceCountry source) => new()
        {
            Name = _pool[source.name_poolid],

            Code = Encoding.ASCII.GetString(source.code, 2),
            ContinentCode = Encoding.ASCII.GetString(source.continent_code, 2)
        };

        #region IEnumerator

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public IEnumerator<IDatabaseCountry> GetEnumerator()
        {
            return ((IEnumerable<DatabaseV1SourceCountry>)this).Select(MapNative).GetEnumerator();
        }

        IEnumerator<DatabaseV1SourceCountry> IEnumerable<DatabaseV1SourceCountry>.GetEnumerator()
        {
            return new ViewEnumerator<DatabaseV1SourceCountry>(_countryView);
        }

        #endregion

        public void Dispose()
        {
            _countryView.Dispose();
        }
    }

    internal class DatabaseV1Country : IDatabaseCountry, ISearchableItem
    {
        object ISearchableItem.Key => Code;

        public string Name { get; init; }
        public string Code { get; init; }
        public string ContinentCode { get; init; }
    }
}