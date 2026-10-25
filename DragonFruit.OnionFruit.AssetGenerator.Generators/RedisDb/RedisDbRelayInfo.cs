// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing.Models.Enums;
using Redis.OM.Modeling;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.RedisDb;

[Document(Prefixes = ["onionfruit-web:nodes"], IndexName = "onionfruit-web:node-idx", StorageType = StorageType.Json)]
public class RedisDbRelayInfo
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

    internal RedisDbRelayInfo Clone() => (RedisDbRelayInfo)MemberwiseClone();
}