// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using StackExchange.Redis;

namespace DragonFruit.OnionFruit.Web.Shared.Redis;

public static class RedisKeys
{
    /// <summary>
    /// Builds a key using the configured prefix, omitting empty segment values.
    /// </summary>
    public static RedisKey Create(RedisOptions options, params string[] segments)
    {
        var parts = new List<string>(segments.Length + 1);

        if (!string.IsNullOrWhiteSpace(options.KeyPrefix))
        {
            var prefix = options.KeyPrefix.Trim().Trim(':');
            if (prefix.Length > 0)
            {
                parts.Add(prefix);
            }
        }

        parts.AddRange(segments.Where(segment => !string.IsNullOrWhiteSpace(segment)));

        return string.Join(':', parts);
    }
}
