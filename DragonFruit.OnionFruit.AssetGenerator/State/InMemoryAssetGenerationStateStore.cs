// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Concurrent;
using DragonFruit.OnionFruit.AssetGenerator.Abstractions;

namespace DragonFruit.OnionFruit.AssetGenerator.State;

internal sealed class InMemoryAssetGenerationStateStore : IAssetGenerationStateStore
{
    private readonly ConcurrentDictionary<string, DateTimeOffset> _lastRuns = new(StringComparer.Ordinal);

    public Task<DateTimeOffset?> GetLastRunAsync(string sourceName, CancellationToken ct)
    {
        return Task.FromResult(_lastRuns.TryGetValue(sourceName, out var timestamp) ? timestamp : (DateTimeOffset?)null);
    }

    public Task SetLastRunAsync(string sourceName, DateTimeOffset timestamp, CancellationToken ct)
    {
        _lastRuns[sourceName] = timestamp;
        return Task.CompletedTask;
    }
}