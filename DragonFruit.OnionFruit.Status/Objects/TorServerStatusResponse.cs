// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using DragonFruit.OnionFruit.Status.Objects.Relay;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status.Objects
{
    public class TorServerStatusResponse
    {
        [JsonProperty("version")]
        public double Version { get; set; }

        [JsonProperty("build_revision")]
        public string BuildHash { get; set; }

        [JsonProperty("relays_published")]
        public DateTimeOffset PublishDate { get; set; }

        [JsonProperty("relays")]
        public IEnumerable<RelayInfo> Relays { get; set; }

        [JsonProperty("relays_truncated")]
        public int RelaysTruncated { get; set; }
    }
}
