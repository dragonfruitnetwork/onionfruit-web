// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO.MemoryMappedFiles;
using System.Net;
using DragonFruit.OnionFruit.Services.LocationDb.Abstractions;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    public class DatabaseV1 : ILocationDatabase
    {
        private readonly MemoryMappedFile _mmdb;

        private readonly DatabaseV1NetworkTree _networkTree;
        private readonly DatabaseV1StringPool _stringPool;
        private readonly DatabaseV1Countries _countries;
        private readonly DatabaseV1Networks _networks;
        private readonly DatabaseV1AS _as;

        private readonly uint _vendorStringLoc, _descriptionStringLoc, _licenseStringLoc;

        private const uint InvalidNetwork = 0xffffffff;

        internal unsafe DatabaseV1(MemoryMappedFile mmdb)
        {
            _mmdb = mmdb;
            DatabaseV1Header header;

            using (var headerView = mmdb.CreateViewAccessor(sizeof(GlobalDatabaseHeader), sizeof(DatabaseV1Header)))
            {
                headerView.Read(0, out header);
            }

            // copy header info over (endianess is corrected at the stringpool)
            _vendorStringLoc = header.vendor;
            _licenseStringLoc = header.license;
            _descriptionStringLoc = header.description;

            // create object views
            var networkTreeView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.network_tree_offset), BinaryUtils.EnsureEndianness(header.network_tree_length));
            _networkTree = new DatabaseV1NetworkTree(networkTreeView);

            var stringPoolView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.pool_offset), BinaryUtils.EnsureEndianness(header.pool_length));
            _stringPool = new DatabaseV1StringPool(stringPoolView);

            var asView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.as_offset), BinaryUtils.EnsureEndianness(header.as_length));
            _as = new DatabaseV1AS(asView, _stringPool);

            var networksView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.network_data_offset), BinaryUtils.EnsureEndianness(header.network_data_length));
            _networks = new DatabaseV1Networks(networksView);

            var countryView = mmdb.CreateViewAccessor(BinaryUtils.EnsureEndianness(header.countries_offset), BinaryUtils.EnsureEndianness(header.countries_length));
            _countries = new DatabaseV1Countries(countryView, _stringPool);
        }

        public int Version => 1;

        public string Vendor => _stringPool[_vendorStringLoc];

        public string License => _stringPool[_licenseStringLoc];

        public string Description => _stringPool[_descriptionStringLoc];

        public IASDatabase AS => _as;
        public INetworkDatabase Networks => _networks;
        public ICountryDatabase Countries => _countries;

        public IAddressLocatedNetwork ResolveAddress(IPAddress address)
        {
            int depth = -1;
            uint nextNodeIndex = 0;
            byte[] mappedAddress = address.MapToIPv6().GetAddressBytes();
            byte[] networkAddress = IPAddress.IPv6None.GetAddressBytes();

            DatabaseSourceNetworkNode node;

            do
            {
                if (nextNodeIndex >= _networkTree.Count)
                {
                    return null;
                }

                // get the bit to perform next node indexing on
                depth++;
                var bit = ((mappedAddress[depth / 8] >> (7 - (depth % 8))) & 1);

                // set the bit in the network address - this can be used to determine the size of the network
                networkAddress[depth / 8] ^= (byte)((-bit ^ networkAddress[depth / 8]) & (1 << (7 - (depth % 8))));

                node = _networkTree.ElementAt(nextNodeIndex);
                nextNodeIndex = BinaryUtils.EnsureEndianness(bit == 0 ? node.zero : node.one);
            } while (nextNodeIndex > 0);

            // todo fix
            var networkPrefixAddress = new IPAddress(networkAddress);
            var prefix = networkPrefixAddress.IsIPv4MappedToIPv6
                ? new NetworkPrefix(networkPrefixAddress.MapToIPv4(), depth - 96)
                : new NetworkPrefix(networkPrefixAddress, depth);

            var networkIndex = BinaryUtils.EnsureEndianness(node.network);
            return networkIndex == InvalidNetwork ? null : _networks.CreateWithPrefix(networkIndex, prefix);
        }

        public void Dispose()
        {
            _stringPool.Dispose();
            _countries.Dispose();

            _mmdb.Dispose();
        }
    }
}
