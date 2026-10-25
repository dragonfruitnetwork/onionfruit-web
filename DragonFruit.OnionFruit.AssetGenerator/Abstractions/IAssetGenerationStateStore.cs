// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Abstractions;

/// <summary>
/// Stores the last successful generation timestamp for each source.
/// </summary>
public interface IAssetGenerationStateStore
{
    Task<DateTimeOffset?> GetLastRunAsync(string sourceName, CancellationToken ct);
    Task SetLastRunAsync(string sourceName, DateTimeOffset timestamp, CancellationToken ct);
}
