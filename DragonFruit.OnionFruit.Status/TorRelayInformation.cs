// OnionFruit.Status Copyright 2021 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status
{
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class TorRelayInformation
    {
        [DataMember(Name = "version")]
        public string Version { get; set; }

        [DataMember(Name = "build_revision")]
        public string BuildRevision { get; set; }

        [DataMember(Name = "relays_published")]
        public DateTimeOffset RelaysPublished { get; set; }

        [DataMember(Name = "relays")]
        public TorRelay[] Relays { get; set; }

        [DataMember(Name = "relays_truncated")]
        public long RelaysTruncated { get; set; }
    }
}
