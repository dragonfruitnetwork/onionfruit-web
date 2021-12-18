using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Api.Status.Objects
{
    [Serializable]
    public class TorClientConnectionHistory
    {
        [JsonPropertyName("fingerprint")]
        public string Fingerprint { get; set; }
        
        [JsonPropertyName("average_clients")]
        public IReadOnlyDictionary<string, TorHistoryGraph> Clients { get; set; }
    }
}