// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Runtime.Serialization;
using DragonFruit.OnionFruit.Api.Converters;
using DragonFruit.OnionFruit.Api.Enums;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Api.Objects
{
    [Serializable, DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class TorBridgeDetails
    {
        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }

        [DataMember(Name = "hashed_fingerprint")]
        public string HashedFingerprint { get; set; }

        [DataMember(Name = "first_seen")]
        public DateTime FirstSeen { get; set; }

        [DataMember(Name = "last_seen")]
        public DateTime LastSeen { get; set; }

        /// <summary>
        /// Whether the Bridge is currently online
        /// </summary>
        [DataMember(Name = "running")]
        public bool Running { get; set; }

        /// <summary>
        /// Features the tor node has
        /// </summary>
        public TorNodeFlags Flags { get; set; }

        [DataMember(Name = "flags")]
        public string[] RawFlags
        {
            get => NodeFlagConverter.ToString(Flags);
            set => Flags = NodeFlagConverter.Parse(value);
        }

        /// <summary>
        /// UTC DateTime the server was last restarted
        /// </summary>
        [DataMember(Name = "last_restarted")]

        public DateTime? LastRestarted { get; set; }

        /// <summary>
        /// Bandwidth, in bytes/sec, the server is capable of providing
        /// </summary>
        [DataMember(Name = "advertised_bandwidth")]
        public long AdvertisedBandwidth { get; set; }

        /// <summary>
        /// The Tor version and
        /// </summary>
        [DataMember(Name = "platform")]
        public string Platform { get; set; }

        /// <summary>
        /// The version of Tor the server is running
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// Whether the bridge is running a recommended version of Tor
        /// </summary>
        [DataMember(Name = "recommended_version")]
        public bool RecommendedVersion { get; set; }

        /// <summary>
        /// The pluggable transport types this bridge supports
        /// </summary>
        [DataMember(Name = "transports")]
        public string[] Transports { get; set; }

        [DataMember(Name = "or_addresses")]
        public string[] OrAddresses { get; set; }

        [DataMember(Name = "bridgedb_distributor")]
        public string BridgeDbDistributor { get; set; }
    }
}
