using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public interface IFileSink
{
    Stream CreateFile(string pathName);
}

public interface IUploadFileSource
{
    Task CopyToFolder(string path);
    Task<Stream> CreateArchiveStream();
}

public class DatabaseFileSink : IFileSink, IDisposable, IUploadFileSource
{
    private readonly IDictionary<string, FileStream> _files = new Dictionary<string, FileStream>();

    public bool HasItems => _files.Any();
    public int FileCount => _files.Count;
    
    public Stream CreateFile(string pathName)
    {
        var tempPath = Path.GetTempFileName();
        var tempStream = File.Create(tempPath, 4096, FileOptions.Asynchronous | FileOptions.SequentialScan | FileOptions.DeleteOnClose);
        
        _files[pathName] = tempStream;
        return tempStream;
    }

    public Task CopyToFolder(string path)
    {
        return CopyFilesTo(name => new FileStream(name, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous));
    }

    public async Task<Stream> CreateArchiveStream()
    {
        var zipStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose);

        using var zipArchive = new ZipArchive(zipStream, ZipArchiveMode.Create, true);
        await CopyFilesTo(n => zipArchive.CreateEntry(n, CompressionLevel.SmallestSize).Open());

        return zipStream;
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
    }
}
