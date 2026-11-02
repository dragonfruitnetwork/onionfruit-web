// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.AssetGenerator.Abstractions;

public sealed record AssetPublishContext(
    string WorkspaceRoot,
    string Version,
    IReadOnlyCollection<string> Assets
);

/// <summary>
/// Publishes the files produced generators after they've been run
/// </summary>
public interface IAssetPublisher
{
    Task PublishAsync(AssetPublishContext context, CancellationToken ct);
}