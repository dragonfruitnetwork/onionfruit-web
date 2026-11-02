// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Models;

namespace DragonFruit.OnionFruit.Web.Services;

public interface IAssetRedirectService
{
    Task<AssetRedirect?> GetLatestAsync(string assetPath, CancellationToken ct = default);
}
