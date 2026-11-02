// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.ComponentModel.DataAnnotations;

namespace DragonFruit.OnionFruit.AssetGenerator.Publishing;

public sealed class S3AssetPublisherOptions
{
    public const string SectionName = "S3";

    [Required]
    public string Endpoint { get; set; } = string.Empty;

    /// <summary>
    /// Region used for request signing. Development object stores usually accept any region.
    /// </summary>
    public string Region { get; set; } = "us-east-1";

    [Required]
    public string AccessKey { get; set; } = string.Empty;

    [Required]
    public string SecretKey { get; set; } = string.Empty;

    [Required]
    public string BucketName { get; set; } = string.Empty;

    /// <summary>
    /// Optional prefix applied to every uploaded key.
    /// </summary>
    public string? KeyPrefix { get; set; }

    /// <summary>
    /// Whether to send an explicit SHA-256 checksum with uploads.
    /// Disable for S3-compatible providers that do not support checksum headers.
    /// </summary>
    public bool UseChecksum { get; set; } = true;

    /// <summary>
    /// Whether to enable payload signing. Set to <c>false</c> for providers that don't support it (i.e. Cloudflare R2)
    /// </summary>
    public bool PayloadSigning { get; set; } = true;

    /// <summary>
    /// Number of days an asset version must be older than before the expiry purge job can remove it from storage.
    /// The currently referenced version of each asset is never removed, regardless of age.
    /// </summary>
    [Range(1, 3650)]
    public int ExpireOldAssetsAfter { get; set; } = 30;
}