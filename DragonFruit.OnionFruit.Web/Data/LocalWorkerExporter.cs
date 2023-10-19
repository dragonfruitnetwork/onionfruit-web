// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace DragonFruit.OnionFruit.Web.Data
{
    /// <summary>
    /// A <see cref="IDataExporter"/> used to copy results directly to the <see cref="LocalAssetStore"/> without requiring additional signalling or uploading
    /// </summary>
    internal class LocalWorkerExporter : IDataExporter
    {
        public async Task PerformUpload(IServiceProvider services, IUploadFileSource source)
        {
            var localAssetStore = services.GetRequiredService<LocalAssetStore>();
            var versionedStore = localAssetStore.CreateNewAssetStoreRevision();

            await source.IterateAllStreams((name, stream) => versionedStore.AddFile(name, stream)).ConfigureAwait(false);
        }
    }
}