using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public interface IDatabaseFileSink
{
    Task WriteFile(string pathName, Stream stream);
}

public class DatabaseFileSink : IDatabaseFileSink, IDisposable
{
    private readonly Stream _sinkStream;
    private readonly ZipArchive _sink;

    private bool _archiveProcessed;

    public DatabaseFileSink()
    {
        _sinkStream = new FileStream(Path.GetTempFileName(), FileMode.Create, FileAccess.ReadWrite, FileShare.None, 4096, FileOptions.Asynchronous | FileOptions.DeleteOnClose);
        _sink = new ZipArchive(_sinkStream, ZipArchiveMode.Create, true);
    }

    public async Task WriteFile(string pathName, Stream stream)
    {
        if (_archiveProcessed)
        {
            throw new InvalidOperationException("Archive has already been processed. Additional files cannot be added.");
        }
        
        var entry = _sink.CreateEntry(pathName);
        
        await using var fileStream = entry.Open();
        await stream.CopyToAsync(fileStream).ConfigureAwait(false);
        
        fileStream.SetLength(fileStream.Position);
    }

    public Stream GetArchive()
    {
        if (!_archiveProcessed)
        {
            _sink.Dispose();
            _archiveProcessed = true;
        }
        
        return _sinkStream;
    }

    public void Dispose()
    {
        _sinkStream?.Dispose();
        _sink?.Dispose();
    }
}