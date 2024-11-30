// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Data;

public interface IRemoteAssetStore
{
    Task<AssetInfo> GetAssetInfo(string fileName);
}

public interface IAssetStore : IRemoteAssetStore
{
    Stream GetReadableFileStream(string relPath);
    IAssetStoreRevision CreateAssetStoreRevision(string revisionId);
}