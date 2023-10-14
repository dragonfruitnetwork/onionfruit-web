// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;
using Redis.OM.Modeling;

namespace DragonFruit.OnionFruit.Web.Worker.Database;

[Document(Prefixes = new[] {"onionfruit-web:nodes"}, IndexName = "onionfruit-web:node-idx", StorageType = StorageType.Json)]
public class OnionFruitNodeInfo
{
    [Indexed]
    [RedisIdField]
    public string IpAddress { get; set; }

    [Indexed(Sortable = true)]
    public long DatabaseVersion { get; set; }

    public string CountryCode { get; set; }
    public string CountryName { get; set; }
    public string ProviderName { get; set; }

    public TorNodeFlags Flags { get; set; }

    internal OnionFruitNodeInfo Clone() => (OnionFruitNodeInfo)MemberwiseClone();
}