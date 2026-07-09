// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace DragonFruit.OnionFruit.Web.Configuration;

public class ServerOptions : IValidatableObject
{
    public const string SectionName = "Server";

    /// <summary>
    /// Whether the data-refresh worker should be hosted inside the server process, serving assets from <see cref="AssetRoot"/>
    /// </summary>
    public bool UseBuiltInWorker { get; set; } = true;

    /// <summary>
    /// Directory versioned assets are written to and served from when the built-in worker is enabled
    /// </summary>
    [Required]
    public string AssetRoot { get; set; } = Path.Combine(".", "onionfruit-web-assets");

    /// <summary>
    /// Format string used to build public asset urls, where {0} is replaced with the versioned file path.
    /// Required when the built-in worker is disabled.
    /// </summary>
    public string RemoteAssetPublicUrl { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (UseBuiltInWorker)
        {
            yield break;
        }

        if (string.IsNullOrEmpty(RemoteAssetPublicUrl))
        {
            yield return new ValidationResult($"{nameof(RemoteAssetPublicUrl)} must be set when {nameof(UseBuiltInWorker)} is disabled", [nameof(RemoteAssetPublicUrl)]);
        }
        else if (!RemoteAssetPublicUrl.Contains("{0}"))
        {
            yield return new ValidationResult($"{nameof(RemoteAssetPublicUrl)} must contain a {{0}} placeholder for the versioned file path", [nameof(RemoteAssetPublicUrl)]);
        }
    }
}
