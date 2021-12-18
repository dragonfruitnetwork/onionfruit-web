// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Text.Json.Serialization;
using DragonFruit.OnionFruit.Api.Converters;

namespace DragonFruit.OnionFruit.Api.Status.Objects
{
    [Serializable]
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
}
