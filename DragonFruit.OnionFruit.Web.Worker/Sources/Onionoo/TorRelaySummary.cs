// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public class TorRelaySummary
{
    /// <summary>
    /// The relay nickname, represented as 1-19 chars
    /// </summary>
    [JsonPropertyName("n")]
    public string Nickname { get; set; }

    /// <summary>
    /// A 40 uppercase-hex identifier
    /// </summary>
    [JsonPropertyName("f")]
    public string Fingerprint { get; set; }

    /// <summary>
    /// Array of IP addresses that the relay accepts routing connections at
    /// </summary>
    [JsonPropertyName("a")]
    public string[] Addresses { get; set; }

    /// <summary>
    /// Whether the relay was running at the last consensus
    /// </summary>
    [JsonPropertyName("r")]
    public bool Running { get; set; }
}