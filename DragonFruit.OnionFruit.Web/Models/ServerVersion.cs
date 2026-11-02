// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Models;

public record ServerVersion([property: JsonPropertyName("v")] string Version);