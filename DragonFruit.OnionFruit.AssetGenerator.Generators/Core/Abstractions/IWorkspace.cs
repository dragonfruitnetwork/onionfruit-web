// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;

public interface IWorkspace
{
    Task WriteAsync(string key, byte[] contents, CancellationToken ct);
    Task<FileStream> CreateFileAsync(string key, CancellationToken ct);
}