// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Runtime.Serialization;
using DragonFruit.OnionFruit.Api.Converters;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Api.Objects
{
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class TorNodeOverloadRateLimit
    {
        [DataMember(Name = "rate-limit")]
        public long RateLimit { get; set; }

        [DataMember(Name = "burst-limit")]
        public long BurstLimit { get; set; }

        [DataMember(Name = "write-count")]
        public int WriteCount { get; set; }

        [DataMember(Name = "read-count")]
        public int ReadCount { get; set; }

        [DataMember(Name = "timestamp")]
        [JsonConverter(typeof(MillisecondEpochConverter))]
        public DateTime Timestamp { get; set; }
    }
}
