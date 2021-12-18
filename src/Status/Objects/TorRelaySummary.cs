using System;
using System.Net;
using System.Text.Json.Serialization;
using DragonFruit.OnionFruit.Api.Converters;

namespace DragonFruit.OnionFruit.Api.Status.Objects
{
    [Serializable]
    public class TorRelaySummary
    {
        /// <summary>
        /// The relay nickname, represented as 1-19 chars
        /// </summary>
        [JsonPropertyName("n")]
        public string Nickname { get; set; }
        
        /// <summary>
        /// A 40 uppercase-hex identifier
        /// </summary>
        [JsonPropertyName("f")]
        public string Fingerprint { get; set; }
        
        /// <summary>
        /// Array of IP addresses that the relay accepts routing connections at
        /// </summary>
        [JsonPropertyName("a")]
        public string[] Addresses { get; set; }
        
        /// <summary>
        /// Whether the relay was running at the last consensus
        /// </summary>
        [JsonPropertyName("r")]
        public bool Running { get; set; }
    }
}