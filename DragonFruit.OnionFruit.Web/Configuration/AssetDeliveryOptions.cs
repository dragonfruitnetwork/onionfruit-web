// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.ComponentModel.DataAnnotations;

namespace DragonFruit.OnionFruit.Web.Configuration;

public sealed class AssetDeliveryOptions : IValidatableObject
{
    public const string SectionName = "Assets";

    [Required]
    public string BaseUrl { get; set; } = string.Empty;

    public string? KeyPrefix { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Uri.TryCreate(BaseUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
        {
            yield return new ValidationResult($"{nameof(BaseUrl)} must be an absolute HTTP or HTTPS URL", [nameof(BaseUrl)]);
        }
    }
}
