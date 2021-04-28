// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status
{
    public class TorRelayInformation
    {
        [JsonProperty("version")]
        public string Version { get; set; }

        [JsonProperty("build_revision")]
        public string BuildRevision { get; set; }

        [JsonProperty("relays_published")]
        public DateTimeOffset RelaysPublished { get; set; }

        [JsonProperty("relays")]
        public TorRelay[] Relays { get; set; }

        [JsonProperty("relays_truncated")]
        public long RelaysTruncated { get; set; }
    }
}
