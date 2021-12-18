// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Api.Status.Objects
{
    [Serializable]
    public class TorBridgeSummary
    {
        /// <summary>
        /// The relay nickname, represented as 1-19 chars
        /// </summary>
        [JsonPropertyName("n")]
        public string Nickname { get; set; }

        /// <summary>
        /// SHA-1 hash of the bridge fingerprint
        /// </summary>
        [JsonPropertyName("h")]
        public string Fingerprint { get; set; }

        /// <summary>
        /// Whether the relay was running at the last consensus
        /// </summary>
        [JsonPropertyName("r")]
        public bool Running { get; set; }
    }
}
