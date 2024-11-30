// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Data
{
    public interface IAssetStoreRevision
    {
        Task AddFile(string fileName, Stream input);
    }
}