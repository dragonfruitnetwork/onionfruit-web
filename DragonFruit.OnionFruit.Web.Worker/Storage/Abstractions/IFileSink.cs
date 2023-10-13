using System.IO;

namespace DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;

public interface IFileSink
{
    /// <summary>
    /// Creates a file to be associated with the current sink and returns a writable stream.
    /// Streams created using this method should NOT be disposed of.
    /// </summary>
    Stream CreateFile(string pathName);
}