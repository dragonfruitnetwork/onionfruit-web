// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Api.Objects
{
    [Serializable]
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
}
