// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.ComponentModel.DataAnnotations;

namespace DragonFruit.OnionFruit.Redis;

public sealed class RedisOptions
{
    public const string SectionName = "Redis";

    [Required]
    public string ConnectionString { get; set; } = "localhost:6379";

    public bool DisableCertificateValidation { get; set; } = false;

    /// <summary>
    /// If set, the TLS certificate must match the thumbprint set here.
    /// This should be a hexadecimal string representing the SHA-1 value.
    /// </summary>
    public string? PinnedCertificateThumbprint { get; set; }

    /// <summary>
    /// Optional prefix applied to every key owned by OnionFruit.
    /// </summary>
    public string? KeyPrefix { get; set; }
}
