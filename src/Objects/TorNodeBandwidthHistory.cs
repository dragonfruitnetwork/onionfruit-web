// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Api.Objects
{
    [Serializable, DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class TorNodeBandwidthHistory
    {
        [DataMember(Name = "fingerprint")]
        public string Fingerprint { get; set; }

        [DataMember(Name = "write_history")]
        public IReadOnlyDictionary<string, TorHistoryGraph> WriteHistory { get; set; }

        [DataMember(Name = "read_history")]
        public IReadOnlyDictionary<string, TorHistoryGraph> ReadHistory { get; set; }

        [DataMember(Name = "overload_ratelimits")]
        public TorNodeOverloadRateLimit OverloadRateLimits { get; set; }
    }
}
