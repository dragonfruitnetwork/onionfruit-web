// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Text.Json.Serialization;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Converters;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public class TorNodeOverloadRateLimit
{
    [JsonPropertyName("rate-limit")]
    public long RateLimit { get; set; }

    [JsonPropertyName("burst-limit")]
    public long BurstLimit { get; set; }

    [JsonPropertyName("write-count")]
    public int WriteCount { get; set; }

    [JsonPropertyName("read-count")]
    public int ReadCount { get; set; }

    [JsonPropertyName("timestamp")]
    [JsonConverter(typeof(MillisecondEpochConverter))]
    public DateTime Timestamp { get; set; }
}