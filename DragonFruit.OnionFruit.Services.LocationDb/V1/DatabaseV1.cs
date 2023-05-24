// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO.MemoryMappedFiles;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    public class DatabaseV1 : ILocationDatabase
    {
        private readonly MemoryMappedFile _mmdb;

        private readonly DatabaseV1StringPool _stringPool;
        private readonly DatabaseV1Countries _countries;
        private readonly DatabaseV1AS _as;

        private readonly uint _vendorStringLoc, _descriptionStringLoc, _licenseStringLoc;

        internal unsafe DatabaseV1(MemoryMappedFile mmdb)
        {
            _mmdb = mmdb;
            DatabaseV1SourceHeader header;

            using (var headerView = mmdb.CreateViewAccessor(sizeof(GlobalDatabaseHeader), sizeof(DatabaseV1SourceHeader)))
            {
                headerView.Read(0, out header);
            }

            // copy header info over (endianess is corrected at the stringpool)
            _vendorStringLoc = header.vendor;
            _licenseStringLoc = header.license;
            _descriptionStringLoc = header.description;

            // create object views
            var stringPoolView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.pool_offset), BinaryUtils.EnsureEndianness(header.pool_length));
            _stringPool = new DatabaseV1StringPool(stringPoolView);

            var asView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.as_offset), BinaryUtils.EnsureEndianness(header.as_length));
            _as = new DatabaseV1AS(asView, _stringPool);

            var countryView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.countries_offset), BinaryUtils.EnsureEndianness(header.countries_length));
            _countries = new DatabaseV1Countries(countryView, _stringPool);
        }

        public int Version => 1;

        public string Vendor => _stringPool[_vendorStringLoc];

        public string License => _stringPool[_licenseStringLoc];

        public string Description => _stringPool[_descriptionStringLoc];

        public IASDatabase AS => _as;
        public ICountryDatabase Countries => _countries;

        public void Dispose()
        {
            _stringPool.Dispose();
            _countries.Dispose();

            _mmdb.Dispose();
        }
    }
}
