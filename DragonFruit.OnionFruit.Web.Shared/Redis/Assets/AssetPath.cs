// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.Web.Shared.Redis.Assets;

public static class AssetPath
{
    /// <summary>
    /// Normalizes an asset path so generator output and incoming requests always share a key.
    /// </summary>
    public static string Normalize(string? assetPath)
    {
        return string.IsNullOrWhiteSpace(assetPath)
            ? string.Empty
            : assetPath.Trim().Replace('\\', '/').TrimStart('/');
    }

    /// <summary>
    /// Whether the path is safe to use as an asset index key and in a redirect.
    /// </summary>
    public static bool IsValid(string? assetPath)
    {
        var normalized = Normalize(assetPath);

        if (normalized.Length is 0 or > 2048)
        {
            return false;
        }

        return normalized.Split('/').All(segment => segment.Length > 0 && segment is not "." and not "..");
    }
}
