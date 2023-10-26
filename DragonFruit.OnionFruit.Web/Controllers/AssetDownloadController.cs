// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Globalization;
using System.Web;
using DragonFruit.OnionFruit.Web.Data;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;

namespace DragonFruit.OnionFruit.Web.Controllers;

[EnableCors]
[Route("~/assets")]
public class AssetDownloadController : ControllerBase
{
    private readonly LocalAssetStore _assetStore;

    public AssetDownloadController(LocalAssetStore assetStore)
    {
        _assetStore = assetStore;
    }

    [HttpGet("{*assetPath}")]
    public IActionResult ResolveAssetPath(string assetPath)
    {
        assetPath = HttpUtility.UrlDecode(assetPath);
        var versionedAsset = _assetStore.GetAssetInfo(assetPath);

        if (versionedAsset == null)
        {
            return NotFound();
        }

        // check last-modified date
        if (DateTimeOffset.TryParse(Request.Headers.IfModifiedSince, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var clientModificationDate) && clientModificationDate >= versionedAsset.CreatedAt)
        {
            return StatusCode(304);
        }

        Response.Headers.LastModified = versionedAsset.CreatedAt.ToString("r");
        return RedirectToAction("DownloadVersionedAsset", new {versionedAssetPath = versionedAsset.VersionedPath});
    }

    [HttpGet("dl/{*versionedAssetPath}")]
    public IActionResult DownloadVersionedAsset(string versionedAssetPath)
    {
        versionedAssetPath = HttpUtility.UrlDecode(versionedAssetPath);
        var fs = _assetStore.GetReadableFileStream(versionedAssetPath);

        if (fs == null)
        {
            return NotFound();
        }

        return File(fs, "application/octet-stream");
    }
}