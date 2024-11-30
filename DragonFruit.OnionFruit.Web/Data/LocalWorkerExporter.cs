// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Storage;

namespace DragonFruit.OnionFruit.Web.Data
{
    /// <summary>
    /// A <see cref="IDataExporter"/> used to copy results directly to the <see cref="LocalAssetStore"/> without requiring additional signalling or uploading
    /// </summary>
    internal class LocalWorkerExporter(IAssetStore localStore) : IDataExporter
    {
        public async Task PerformUpload(IUploadFileSource source)
        {
            var versionedStore = localStore.CreateAssetStoreRevision(source.Version);
            await source.IterateAllStreams((name, stream) => versionedStore.AddFile(name, stream)).ConfigureAwait(false);
        }
    }
}