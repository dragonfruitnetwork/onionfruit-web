// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.ComponentModel.DataAnnotations;

namespace DragonFruit.OnionFruit.AssetGenerator.Worker.Configuration;

public sealed class RedisStateStoreOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; set; } = "localhost:6379";

    public string? KeyPrefix { get; set; }
}
