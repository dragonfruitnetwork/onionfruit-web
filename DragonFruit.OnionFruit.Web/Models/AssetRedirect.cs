// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

namespace DragonFruit.OnionFruit.Web.Models;

public sealed record AssetRedirect(
    string AssetPath,
    string Version,
    DateTimeOffset? CreatedAt,
    string ETag,
    Uri Location);