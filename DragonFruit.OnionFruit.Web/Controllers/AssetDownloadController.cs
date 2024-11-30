// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Web;
using DragonFruit.OnionFruit.Web.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

namespace DragonFruit.OnionFruit.Web.Controllers;

[EnableCors]
public class AssetDownloadController(IRemoteAssetStore assetStore) : ControllerBase
{
    [HttpGet("~/assets/{*assetPath}")]
    public async Task<IActionResult> ResolveAssetPath(string assetPath)
    {
        assetPath = HttpUtility.UrlDecode(assetPath);
        var versionedAsset = await assetStore.GetAssetInfo(assetPath);

        if (versionedAsset == null)
        {
            return NotFound();
        }

        // check last-modified date
        if (DateTimeOffset.TryParse(Request.Headers.IfModifiedSince, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var clientModificationDate) && clientModificationDate >= versionedAsset.CreatedAt)
        {
            return StatusCode(304);
        }

        Response.Headers.ETag = new StringValues(versionedAsset.ETag);
        Response.Headers.LastModified = versionedAsset.CreatedAt.ToString("r");

        if (assetStore is not IAssetStore && versionedAsset.VersionedPath.StartsWith("http"))
        {
            return Redirect(versionedAsset.VersionedPath);
        }

        return Redirect(HttpUtility.UrlDecode(Url.Action("DownloadVersionedAsset", new {versionedAssetPath = versionedAsset.VersionedPath})));
    }

    [HttpGet("~/asset-dl/{*versionedAssetPath}")]
    public IActionResult DownloadVersionedAsset(string versionedAssetPath)
    {
        if (assetStore is not IAssetStore store)
        {
            return StatusCode(500);
        }

        versionedAssetPath = HttpUtility.UrlDecode(versionedAssetPath);
        var fs = store.GetReadableFileStream(versionedAssetPath);

        if (fs == null)
        {
            return NotFound();
        }

        return File(fs, "application/octet-stream", Path.GetFileName(versionedAssetPath));
    }
}