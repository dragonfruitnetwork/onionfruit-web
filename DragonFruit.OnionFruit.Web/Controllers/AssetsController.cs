// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Globalization;
using System.Web;
using DragonFruit.OnionFruit.Web.Services;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DragonFruit.OnionFruit.Web.Controllers;

[EnableCors]
[ApiController, Route("assets")]
public sealed class AssetsController(IAssetRedirectService assetRedirectService) : ControllerBase
{
    [HttpGet("{**assetPath}")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status304NotModified)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ResolveAssetPath(string assetPath, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(assetPath))
        {
            return NotFound();
        }

        assetPath = HttpUtility.UrlDecode(assetPath);

        var asset = await assetRedirectService.GetLatestAsync(assetPath, ct);
        if (asset is null)
        {
            return NotFound();
        }

        if (asset.CreatedAt is { } createdAt &&
            DateTimeOffset.TryParse(Request.Headers["If-Modified-Since"].ToString(), CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var clientModificationDate) &&
            clientModificationDate >= createdAt)
        {
            return StatusCode(StatusCodes.Status304NotModified);
        }

        if (asset.CreatedAt is { } lastModified)
        {
            Response.Headers["Last-Modified"] = lastModified.ToString("r", CultureInfo.InvariantCulture);
        }

        // The redirect may be to a different domain. Expose the target so browser CORS requests can read it.
        Response.Headers["X-Asset-Location"] = asset.Location.AbsoluteUri;
        Response.Headers["ETag"] = asset.ETag;

        return Redirect(asset.Location.AbsoluteUri);
    }
}
