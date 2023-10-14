// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

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