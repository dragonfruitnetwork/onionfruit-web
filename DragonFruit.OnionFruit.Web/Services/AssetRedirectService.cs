// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Concurrent;
using System.Globalization;
using DragonFruit.OnionFruit.Web.Shared.Redis.Assets;
using DragonFruit.OnionFruit.Web.Configuration;
using DragonFruit.OnionFruit.Web.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DragonFruit.OnionFruit.Web.Services;

internal sealed class AssetRedirectService(IAssetIndex assetIndex, IMemoryCache cache, IOptions<AssetDeliveryOptions> options) : IAssetRedirectService
{
    private const string CacheKeyPrefix = "asset-redirect:";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    private readonly ConcurrentDictionary<string, Lazy<Task<CachedAssetRedirect>>> _inFlight = new(StringComparer.Ordinal);

    public async Task<AssetRedirect?> GetLatestAsync(string assetPath, CancellationToken ct = default)
    {
        var path = AssetPath.Normalize(assetPath);

        if (!AssetPath.IsValid(path))
        {
            return null;
        }

        var cacheKey = CacheKeyPrefix + path;
        if (cache.TryGetValue(cacheKey, out CachedAssetRedirect? cached) && cached is not null)
        {
            return cached.Value;
        }

        return (await GetOrCreateAsync(cacheKey, path, ct)).Value;
    }

    private async Task<CachedAssetRedirect> GetOrCreateAsync(string cacheKey, string assetPath, CancellationToken ct)
    {
        while (true)
        {
            if (_inFlight.TryGetValue(cacheKey, out var existing))
            {
                return await existing.Value.WaitAsync(ct);
            }

            var lookup = new Lazy<Task<CachedAssetRedirect>>(() => ResolveAndCacheAsync(cacheKey, assetPath));

            if (!_inFlight.TryAdd(cacheKey, lookup))
            {
                continue;
            }

            try
            {
                return await lookup.Value.WaitAsync(ct);
            }
            finally
            {
                _inFlight.TryRemove(new KeyValuePair<string, Lazy<Task<CachedAssetRedirect>>>(cacheKey, lookup));
            }
        }
    }

    private async Task<CachedAssetRedirect> ResolveAndCacheAsync(string cacheKey, string assetPath)
    {
        var result = await ResolveAsync(assetPath, CancellationToken.None);

        cache.Set(cacheKey, result, new MemoryCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = CacheDuration,
            Size = 1
        });

        return result;
    }

    private async Task<CachedAssetRedirect> ResolveAsync(string assetPath, CancellationToken ct)
    {
        var version = await assetIndex.GetLatestVersionAsync(assetPath, ct);
        if (string.IsNullOrWhiteSpace(version))
        {
            return new CachedAssetRedirect(null);
        }

        return new CachedAssetRedirect(new AssetRedirect(assetPath, version, ParseVersionTimestamp(version), $"\"{version}\"", BuildAssetUrl(options.Value, version, assetPath)));
    }

    private static DateTimeOffset? ParseVersionTimestamp(string version)
    {
        return long.TryParse(version, NumberStyles.Integer, CultureInfo.InvariantCulture, out var timestamp)
            ? DateTimeOffset.FromUnixTimeSeconds(timestamp)
            : null;
    }

    private static Uri BuildAssetUrl(AssetDeliveryOptions options, string version, string assetPath)
    {
        var segments = new List<string>();

        if (!string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            segments.AddRange(SplitPath(options.KeyPrefix));
        }

        segments.Add(version);
        segments.AddRange(SplitPath(assetPath));

        var escapedPath = string.Join('/', segments.Select(Uri.EscapeDataString));
        return new Uri($"{options.BaseUrl.TrimEnd('/')}/{escapedPath}", UriKind.Absolute);
    }

    private static IEnumerable<string> SplitPath(string path)
    {
        return path.Trim('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
    }

    private sealed record CachedAssetRedirect(AssetRedirect? Value);
}
