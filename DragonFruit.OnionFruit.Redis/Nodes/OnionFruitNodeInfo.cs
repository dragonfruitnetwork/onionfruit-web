// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using Redis.OM.Modeling;

namespace DragonFruit.OnionFruit.Redis.Nodes;

[Document(Prefixes = ["onionfruit-web:nodes"], IndexName = "onionfruit-web:node-idx", StorageType = StorageType.Json)]
public class OnionFruitNodeInfo
{
    [Indexed]
    [RedisIdField]
    public required string IpAddress { get; set; }

    [Indexed(Sortable = true)]
    public required long DatabaseVersion { get; set; }

    public string? CountryCode { get; set; }
    public string? CountryName { get; set; }

    public int? ProviderNumber { get; set; }
    public string? ProviderName { get; set; }

    public TorNodeFlags Flags { get; set; }

    public OnionFruitNodeInfo Clone() => (OnionFruitNodeInfo)MemberwiseClone();
}
