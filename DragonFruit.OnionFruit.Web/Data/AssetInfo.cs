// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using JetBrains.Annotations;

namespace DragonFruit.OnionFruit.Web.Data
{
    [UsedImplicitly]
    public record AssetInfo(string Name, string VersionedPath, DateTimeOffset CreatedAt, string ETag);
}