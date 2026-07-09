// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.ComponentModel.DataAnnotations;

namespace DragonFruit.OnionFruit.Web.Worker.Configuration;

public class S3StorageOptions
{
    public const string SectionName = "S3";

    /// <summary>
    /// AWS region system name (e.g. eu-west-2). If set, takes priority over <see cref="Endpoint"/>.
    /// </summary>
    public string Region { get; set; }

    /// <summary>
    /// Custom service url, for use with S3-compatible providers
    /// </summary>
    public string Endpoint { get; set; }

    [Required]
    public string AccessKey { get; set; }

    [Required]
    public string SecretKey { get; set; }

    [Required]
    public string BucketName { get; set; }

    /// <summary>
    /// Expiry time, in days, after which superseded assets are removed from the bucket
    /// </summary>
    [Range(1, double.MaxValue)]
    public double ExpireOldAssetsAfter { get; set; } = 30;
}
