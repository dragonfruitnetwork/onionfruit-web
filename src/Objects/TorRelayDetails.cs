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
    public class TorRelayDetails
    {
        [DataMember(Name = "nickname")]
        public string Nickname { get; set; }

        [DataMember(Name = "fingerprint")]
        public string Fingerprint { get; set; }

        [DataMember(Name = "first_seen")]
        public DateTime FirstSeen { get; set; }

        [DataMember(Name = "last_seen")]
        public DateTime LastSeen { get; set; }

        [DataMember(Name = "last_changed_address_or_port")]
        public DateTime LastChangedAddressOrPort { get; set; }

        /// <summary>
        /// <see cref="DateTime"/> the server was last restarted
        /// </summary>
        [DataMember(Name = "last_restarted")]
        public DateTime? LastRestarted { get; set; }

        /// <summary>
        /// Whether the node is running
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
        /// The country the server is located in
        /// </summary>
        [DataMember(Name = "country")]
        public string CountryCode { get; set; }

        /// <summary>
        /// The country name the server is located in
        /// </summary>
        [DataMember(Name = "country_name")]
        public string CountryName { get; set; }

        /// <summary>
        /// The autonomous system identifier, as assigned to by IANA
        /// </summary>
        [DataMember(Name = "as")]
        public string ASN { get; set; }

        /// <summary>
        /// The autonomous system name, as assigned to by IANA
        /// </summary>
        [DataMember(Name = "as_name")]
        public string ASName { get; set; }

        /// <summary>
        /// Weight assigned to the relay that clients use in their path selection algorithm
        /// </summary>
        [DataMember(Name = "consensus_weight")]
        public long ConsensusWeight { get; set; }

        /// <summary>
        /// Bandwidth, in bytes/sec, that the server can handle over a long period of time
        /// </summary>
        [DataMember(Name = "bandwidth_rate")]
        public long? BandwidthRate { get; set; }

        /// <summary>
        /// Bandwidth, in bytes/sec, that the server can handle in a very short period of time (burst)
        /// </summary>
        [DataMember(Name = "bandwidth_burst")]
        public long? BandwidthBurst { get; set; }

        /// <summary>
        /// Bandwidth, in bytes/sec, that the server has provided based on the last 24 hours of activity
        /// </summary>
        [DataMember(Name = "observed_bandwidth")]
        public long? ObservedBandwidth { get; set; }

        /// <summary>
        /// Bandwidth, in bytes/sec, that the server can theoretically provide
        /// </summary>
        [DataMember(Name = "advertised_bandwidth")]
        public long? AdvertisedBandwidth { get; set; }

        /// <summary>
        /// Exit policy lines
        /// </summary>
        [DataMember(Name = "exit_policy")]
        public string[] ExitPolicy { get; set; }

        /// <summary>
        /// Contact info for the owner of the server
        /// </summary>
        [DataMember(Name = "contact")]
        public string Contact { get; set; }

        /// <summary>
        /// The currently running platform (Tor version and Operating System)
        /// </summary>
        [DataMember(Name = "platform")]
        public string Platform { get; set; }

        /// <summary>
        /// Tor software version running on the relay
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// Likelihood this server will be selected as an entry node in a new circuit
        /// </summary>
        [DataMember(Name = "guard_probability")]
        public double GuardProbability { get; set; }

        /// <summary>
        /// Likelihood this server will be selected as a middleman node in a new circuit
        /// </summary>
        [DataMember(Name = "middle_probability")]
        public double MiddleProbability { get; set; }

        /// <summary>
        /// Likelihood this server will be selected as an exit node in a new circuit
        /// </summary>
        [DataMember(Name = "exit_probability")]
        public double ExitProbability { get; set; }

        /// <summary>
        /// Whether the relay is running a recommended version of the Tor software
        /// </summary>
        [DataMember(Name = "recommended_version")]
        public bool RecommendedVersion { get; set; }

        /// <summary>
        /// IPv4 address and TCP port where the relay accepts connections
        /// </summary>
        [DataMember(Name = "dir_address")]
        public string DirAddress { get; set; }

        [DataMember(Name = "or_addresses")]
        public string[] OrAddresses { get; set; }

        [DataMember(Name = "exit_addresses")]
        public string[] ExitAddresses { get; set; }

        [DataMember(Name = "verified_host_names")]
        public string[] VerifiedHostNames { get; set; }

        [DataMember(Name = "unverified_host_names")]
        public string[] UnverifiedHostNames { get; set; }
    }
}
