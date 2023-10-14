// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public class TorNodeBandwidthHistory
{
    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; set; }

    [JsonPropertyName("write_history")]
    public IReadOnlyDictionary<string, TorHistoryGraph> WriteHistory { get; set; }

    [JsonPropertyName("read_history")]
    public IReadOnlyDictionary<string, TorHistoryGraph> ReadHistory { get; set; }

    [JsonPropertyName("overload_ratelimits")]
    public TorNodeOverloadRateLimit OverloadRateLimits { get; set; }
}