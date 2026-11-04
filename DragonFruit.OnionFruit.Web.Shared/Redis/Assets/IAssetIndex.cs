// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.Web.Shared.Redis.Assets;

/// <summary>
/// Stores the latest published version for each asset.
/// </summary>
public interface IAssetIndex
{
    /// <summary>
    /// Gets the latest version of an asset, or <see langword="null"/> if the asset has never been published.
    /// </summary>
    Task<string?> GetLatestVersionAsync(string assetPath, CancellationToken ct = default);

    /// <summary>
    /// Gets every recorded asset-to-version mapping.
    /// </summary>
    Task<IReadOnlyDictionary<string, string>> GetAllVersionsAsync(CancellationToken ct = default);

    /// <summary>
    /// Records the latest version of an asset.
    /// </summary>
    Task SetLatestVersionAsync(string assetPath, string version, CancellationToken ct = default);

    /// <summary>
    /// Records the latest version of multiple assets in a single operation.
    /// </summary>
    Task SetLatestVersionsAsync(IEnumerable<KeyValuePair<string, string>> versions, CancellationToken ct = default);
}
