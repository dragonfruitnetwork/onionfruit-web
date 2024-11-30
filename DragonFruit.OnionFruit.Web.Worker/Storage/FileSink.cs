// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

/// <summary>
/// Represents a collection of files that can be copied or compressed.
/// </summary>
public class FileSink(string version) : IUploadFileSource, IDisposable
{
    private readonly IDictionary<string, FileStream> _files = new Dictionary<string, FileStream>();

    public string Version { get; } = version;

    public bool HasItems => _files.Any();

    public Stream CreateFile(string pathName)
    {
        var tempPath = Path.GetTempFileName();
        var tempStream = File.Create(tempPath, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);

        _files[pathName] = tempStream;
        return tempStream;
    }

    public Task CopyToFolderAsync(string path) => CopyFilesTo(name =>
    {
        var fullPath = Path.Combine(path, name);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath));
        return new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
    });

    public async Task IterateAllStreams(Func<string, FileStream, Task> iterator)
    {
        foreach (var (name, stream) in _files)
        {
            stream.Seek(0, SeekOrigin.Begin);
            await iterator.Invoke(name, stream).ConfigureAwait(false);
        }
    }

    private async Task CopyFilesTo(Func<string, Stream> streamSelector)
    {
        foreach (var (name, content) in _files)
        {
            await using var entry = streamSelector.Invoke(name);

            content.Seek(0, SeekOrigin.Begin);
            await content.CopyToAsync(entry).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        foreach (var file in _files.Values)
        {
            file.Dispose();
        }

        _files.Clear();
    }
}