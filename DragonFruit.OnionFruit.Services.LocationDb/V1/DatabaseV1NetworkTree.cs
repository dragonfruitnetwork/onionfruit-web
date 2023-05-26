// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.IO.MemoryMappedFiles;

namespace DragonFruit.OnionFruit.Services.LocationDb.V1
{
    internal class DatabaseV1NetworkTree : DatabaseV1Collection<DatabaseSourceNetworkNode>
    {
        public DatabaseV1NetworkTree(MemoryMappedViewAccessor view)
            : base(view)
        {
        }
    }
}
