// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Text.Json.Serialization;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Converters;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public class TorRelayDetails
{
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; set; }

    [JsonPropertyName("first_seen")]
    public DateTime FirstSeen { get; set; }

    [JsonPropertyName("last_seen")]
    public DateTime LastSeen { get; set; }

    [JsonPropertyName("last_changed_address_or_port")]
    public DateTime LastChangedAddressOrPort { get; set; }

    /// <summary>
    /// <see cref="DateTime"/> the server was last restarted
    /// </summary>
    [JsonPropertyName("last_restarted")]
    public DateTime? LastRestarted { get; set; }

    /// <summary>
    /// Whether the node is running
    /// </summary>
    [JsonPropertyName("running")]
    public bool Running { get; set; }

    /// <summary>
    /// Features the tor node has
    /// </summary>
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
    public string CountryCode { get; set; }

    /// <summary>
    /// The country name the server is located in
    /// </summary>
    [JsonPropertyName("country_name")]
    public string CountryName { get; set; }

    /// <summary>
    /// The autonomous system identifier, as assigned to by IANA
    /// </summary>
    [JsonPropertyName("as")]
    public string ASN { get; set; }

    /// <summary>
    /// The autonomous system name, as assigned to by IANA
    /// </summary>
    [JsonPropertyName("as_name")]
    public string ASName { get; set; }

    /// <summary>
    /// Weight assigned to the relay that clients use in their path selection algorithm
    /// </summary>
    [JsonPropertyName("consensus_weight")]
    public long ConsensusWeight { get; set; }

    /// <summary>
    /// Bandwidth, in bytes/sec, that the server can handle over a long period of time
    /// </summary>
    [JsonPropertyName("bandwidth_rate")]
    public long? BandwidthRate { get; set; }

    /// <summary>
    /// Bandwidth, in bytes/sec, that the server can handle in a very short period of time (burst)
    /// </summary>
    [JsonPropertyName("bandwidth_burst")]
    public long? BandwidthBurst { get; set; }

    /// <summary>
    /// Bandwidth, in bytes/sec, that the server has provided based on the last 24 hours of activity
    /// </summary>
    [JsonPropertyName("observed_bandwidth")]
    public long? ObservedBandwidth { get; set; }

    /// <summary>
    /// Bandwidth, in bytes/sec, that the server can theoretically provide
    /// </summary>
    [JsonPropertyName("advertised_bandwidth")]
    public long? AdvertisedBandwidth { get; set; }

    /// <summary>
    /// Exit policy lines
    /// </summary>
    [JsonPropertyName("exit_policy")]
    public string[] ExitPolicy { get; set; }

    /// <summary>
    /// Contact info for the owner of the server
    /// </summary>
    [JsonPropertyName("contact")]
    public string Contact { get; set; }

    /// <summary>
    /// The currently running platform (Tor version and Operating System)
    /// </summary>
    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    /// <summary>
    /// Tor software version running on the relay
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Likelihood this server will be selected as an entry node in a new circuit
    /// </summary>
    [JsonPropertyName("guard_probability")]
    public double GuardProbability { get; set; }

    /// <summary>
    /// Likelihood this server will be selected as a middleman node in a new circuit
    /// </summary>
    [JsonPropertyName("middle_probability")]
    public double MiddleProbability { get; set; }

    /// <summary>
    /// Likelihood this server will be selected as an exit node in a new circuit
    /// </summary>
    [JsonPropertyName("exit_probability")]
    public double ExitProbability { get; set; }

    /// <summary>
    /// Whether the relay is running a recommended version of the Tor software
    /// </summary>
    [JsonPropertyName("recommended_version")]
    public bool RecommendedVersion { get; set; }

    /// <summary>
    /// IPv4 address and TCP port where the relay accepts connections
    /// </summary>
    [JsonPropertyName("dir_address")]
    public string DirAddress { get; set; }

    [JsonPropertyName("or_addresses")]
    public string[] OrAddresses { get; set; }

    [JsonPropertyName("exit_addresses")]
    public string[] ExitAddresses { get; set; }

    [JsonPropertyName("verified_host_names")]
    public string[] VerifiedHostNames { get; set; }

    [JsonPropertyName("unverified_host_names")]
    public string[] UnverifiedHostNames { get; set; }
}