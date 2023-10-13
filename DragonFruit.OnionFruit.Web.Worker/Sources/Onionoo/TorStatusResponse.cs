// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public class TorStatusResponse<T> : TorStatusResponse<T, T>
{
}

public class TorStatusResponse<TRelay, TBridge>
{
    /// <summary>
    /// The current api protocol
    /// </summary>
    [JsonPropertyName("version")]
    public string Version { get; set; }

    /// <summary>
    /// When not-null, indicates when the next major version will be deployed
    /// </summary>
    [JsonPropertyName("next_major_version_scheduled")]
    public DateTime? NextVersionScheduled { get; set; }

    /// <summary>
    /// Git revision of the software used to write this response. This is omitted if unknown.
    /// </summary>
    [JsonPropertyName("build_revision")]
    public string BuildRevision { get; set; }

    /// <summary>
    /// <see cref="DateTime"/> when the last known relay network status consensus started being valid.
    /// </summary>
    /// <remarks>
    /// Indicates how recent the relay objects in this document are.
    /// </remarks>
    [JsonPropertyName("relays_published")]
    public DateTime RelaysPublished { get; set; }

    /// <summary>
    /// Numbers of relays skipped, if an offset was requested
    /// </summary>
    [JsonPropertyName("relays_skipped")]
    public int? RelaysSkipped { get; set; }

    [JsonPropertyName("relays")]
    public TRelay[] Relays { get; set; }

    /// <summary>
    /// Number of relays omitted due to user page limiting
    /// </summary>
    [JsonPropertyName("relays_truncated")]
    public int RelaysTruncated { get; set; }

    /// <summary>
    /// <see cref="DateTime"/> when the last known relay network status consensus started being valid.
    /// </summary>
    /// <remarks>
    /// Indicates how recent the relay objects in this document are.
    /// </remarks>
    [JsonPropertyName("bridges_published")]
    public DateTime BridgesPublished { get; set; }

    /// <summary>
    /// Numbers of bridges skipped, if an offset was requested
    /// </summary>
    [JsonPropertyName("bridges_skipped")]
    public int? BridgesSkipped { get; set; }

    [JsonPropertyName("bridges")]
    public TBridge[] Bridges { get; set; }

    /// <summary>
    /// Number of bridges omitted due to user page limiting
    /// </summary>
    [JsonPropertyName("bridges_truncated")]
    public int BridgesTruncated { get; set; }
}