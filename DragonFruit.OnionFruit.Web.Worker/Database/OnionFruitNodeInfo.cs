using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;
using Redis.OM.Modeling;

namespace DragonFruit.OnionFruit.Web.Worker.Database;

[Document(Prefixes = new[] {"yuna:onionfruit-nodes"}, IndexName = "yuna:onionfruit-nodes-idx", StorageType = StorageType.Json)]
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