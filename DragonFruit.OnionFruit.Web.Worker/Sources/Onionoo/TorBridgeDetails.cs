﻿// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Text.Json.Serialization;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Converters;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public class TorBridgeDetails
{
    [JsonPropertyName("nickname")]
    public string Nickname { get; set; }

    [JsonPropertyName("hashed_fingerprint")]
    public string HashedFingerprint { get; set; }

    [JsonPropertyName("first_seen")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime FirstSeen { get; set; }

    [JsonPropertyName("last_seen")]
    [JsonConverter(typeof(DateTimeConverter))]
    public DateTime LastSeen { get; set; }

    /// <summary>
    /// Whether the Bridge is currently online
    /// </summary>
    [JsonPropertyName("running")]
    public bool Running { get; set; }

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
    /// UTC DateTime the server was last restarted
    /// </summary>
    [JsonPropertyName("last_restarted")]
    [JsonConverter(typeof(DateTimeConverter))]

    public DateTime? LastRestarted { get; set; }

    /// <summary>
    /// Bandwidth, in bytes/sec, the server is capable of providing
    /// </summary>
    [JsonPropertyName("advertised_bandwidth")]
    public long AdvertisedBandwidth { get; set; }

    /// <summary>
    /// The Tor version and
    /// </summary>
    [JsonPropertyName("platform")]
    public string Platform { get; set; }

    /// <summary>
    /// The version of Tor the server is running
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// Whether the bridge is running a recommended version of Tor
    /// </summary>
    [JsonPropertyName("recommended_version")]
    public bool RecommendedVersion { get; set; }

    /// <summary>
    /// The pluggable transport types this bridge supports
    /// </summary>
    [JsonPropertyName("transports")]
    public string[] Transports { get; set; }

    [JsonPropertyName("or_addresses")]
    public string[] OrAddresses { get; set; }

    [JsonPropertyName("bridgedb_distributor")]
    public string BridgeDbDistributor { get; set; }
}