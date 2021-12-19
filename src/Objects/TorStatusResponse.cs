// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Runtime.Serialization;

namespace DragonFruit.OnionFruit.Api.Objects
{
    [Serializable, DataContract]
    public class TorStatusResponse<TRelay, TBridge>
    {
        /// <summary>
        /// The current api protocol
        /// </summary>
        [DataMember(Name = "version")]
        public string Version { get; set; }

        /// <summary>
        /// When not-null, indicates when the next major version will be deployed
        /// </summary>
        [DataMember(Name = "next_major_version_scheduled")]
        public DateTime? NextVersionScheduled { get; set; }

        /// <summary>
        /// Git revision of the software used to write this response. This is omitted if unknown.
        /// </summary>
        [DataMember(Name = "build_revision")]
        public string BuildRevision { get; set; }

        /// <summary>
        /// <see cref="DateTime"/> when the last known relay network status consensus started being valid.
        /// </summary>
        /// <remarks>
        /// Indicates how recent the relay objects in this document are.
        /// </remarks>
        [DataMember(Name = "relays_published")]
        public DateTime RelaysPublished { get; set; }

        /// <summary>
        /// Numbers of relays skipped, if an offset was requested
        /// </summary>
        [DataMember(Name = "relays_skipped")]
        public int? RelaysSkipped { get; set; }

        [DataMember(Name = "relays")]
        public TRelay[] Relays { get; set; }

        /// <summary>
        /// Number of relays omitted due to user page limiting
        /// </summary>
        [DataMember(Name = "relays_truncated")]
        public int RelaysTruncated { get; set; }

        /// <summary>
        /// <see cref="DateTime"/> when the last known relay network status consensus started being valid.
        /// </summary>
        /// <remarks>
        /// Indicates how recent the relay objects in this document are.
        /// </remarks>
        [DataMember(Name = "bridges_published")]
        public DateTime BridgesPublished { get; set; }

        /// <summary>
        /// Numbers of bridges skipped, if an offset was requested
        /// </summary>
        [DataMember(Name = "bridges_skipped")]
        public int? BridgesSkipped { get; set; }

        [DataMember(Name = "bridges")]
        public TBridge[] Bridges { get; set; }

        /// <summary>
        /// Number of bridges omitted due to user page limiting
        /// </summary>
        [DataMember(Name = "bridges_truncated")]
        public int BridgesTruncated { get; set; }
    }

    [Serializable, DataContract]
    public class TorStatusResponse<T> : TorStatusResponse<T, T>
    {
    }
}
