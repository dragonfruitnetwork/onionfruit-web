// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.IO;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public interface IUploadFileSource
{
    /// <summary>
    /// Gets the version identifier of the contained data
    /// </summary>
    public string Version { get; }

    /// <summary>
    /// Copies the contents of the <see cref="FileSink"/> to the specified path.
    /// Missing directories will be created by the function.
    /// </summary>
    Task CopyToFolderAsync(string path);

    /// <summary>
    /// Iterates all files, providing direct access to the underlying streams.
    /// </summary>
    /// <remarks>
    /// Streams provided by this method should not dispose of them (as they could be consumed by others in the future)
    /// </remarks>
    Task IterateAllStreams(Func<string, FileStream, Task> iterator);
}