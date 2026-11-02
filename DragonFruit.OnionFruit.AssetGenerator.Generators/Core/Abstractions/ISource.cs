// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;

public interface ISource
{
    string Name { get; }

    IReadOnlySet<string> OutputNames { get; }
    IReadOnlyDictionary<string, ISourceOutput> Outputs { get; }

    Task<bool> HasDataChangedAsync(DateTimeOffset lastVersionDate, CancellationToken ct);
    Task FetchAsync(CancellationToken ct);
}