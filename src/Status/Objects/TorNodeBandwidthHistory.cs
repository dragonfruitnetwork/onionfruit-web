using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Api.Status.Objects
{
    [Serializable]
    public class TorNodeBandwidthHistory
    {
        [JsonPropertyName("fingerprint")]
        public string Fingerprint { get; set; }
        
        [JsonPropertyName("write_history")]
        public IReadOnlyDictionary<string, TorHistoryGraphObject> WriteHistory { get; set; }
        
        [JsonPropertyName("read_history")]
        public IReadOnlyDictionary<string, TorHistoryGraphObject> ReadHistory { get; set; }
        
        [JsonPropertyName("overload_ratelimits")]
        public TorNodeOverloadRateLimit OverloadRateLimits { get; set; }
    }
}