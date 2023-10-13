using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace DragonFruit.OnionFruit.Web.Worker.Storage;

public interface IUploadFileSource
{
    /// <summary>
    /// Copies the contents of the <see cref="FileSink"/> to the specified path.
    /// Missing directories will be created by the function.
    /// </summary>
    Task CopyToFolderAsync(string path);
    
    /// <summary>
    /// Creates a new <see cref="ZipArchive"/> and returns a stream containing the compressed files.
    /// </summary>
    Task<Stream> CreateArchiveStreamAsync(CompressionLevel compressionLevel = CompressionLevel.SmallestSize);
}