// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Text.Json.Serialization;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing.Models.Converters;
using DragonFruit.OnionFruit.Redis.Nodes;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing.Models;

public sealed class TorRelayDetails
{
    /// <summary>
    /// Features the tor node has
    /// </summary>
    [JsonIgnore]
    public TorNodeFlags Flags { get; set; }

    [JsonPropertyName("flags")]
    public string[] RawFlags
    {
        get => NodeFlagConverter.ToString(Flags);
        set => Flags = NodeFlagConverter.Parse(value);
    }

    /// <summary>
    /// The country the server is located in
    /// </summary>
    [JsonPropertyName("country")]
    public string? CountryCode { get; set; }

    /// <summary>
    /// The country name the server is located in
    /// </summary>
    [JsonPropertyName("country_name")]
    public string? CountryName { get; set; }

    /// <summary>
    /// The autonomous system identifier, as assigned to by IANA
    /// </summary>
    [JsonPropertyName("as")]
    public string? ASN { get; set; }

    /// <summary>
    /// The autonomous system name, as assigned to by IANA
    /// </summary>
    [JsonPropertyName("as_name")]
    public string? ASName { get; set; }

    /// <summary>
    /// Whether the relay is running a recommended version of the Tor software
    /// </summary>
    [JsonPropertyName("recommended_version")]
    public bool RecommendedVersion { get; set; }

    [JsonPropertyName("or_addresses")]
    public string[] OrAddresses { get; set; }

    [JsonPropertyName("exit_addresses")]
    public string[] ExitAddresses { get; set; }
}