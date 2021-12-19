// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Api.Objects
{
    [Serializable]
    [DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class TorHistoryGraph
    {
        [DataMember(Name = "first")]
        public DateTime Start { get; set; }

        [DataMember(Name = "last")]
        public DateTime End { get; set; }

        [DataMember(Name = "interval")]
        public int IntervalSeconds { get; set; }

        /// <summary>
        /// The <see cref="TimeSpan"/> between two data points
        /// </summary>
        [JsonIgnore]
        public TimeSpan Interval => TimeSpan.FromSeconds(IntervalSeconds);

        /// <summary>
        /// The scale factor each value needs to be multiplied by to get the original value
        /// </summary>
        [DataMember(Name = "factor")]
        public double ScaleFactor { get; set; }

        /// <summary>
        /// The normalised values, as sent by the api
        /// </summary>
        [DataMember(Name = "values")]
        public int?[] NormalisedValues { get; set; }

        /// <summary>
        /// The values, multiplied out by the <see cref="ScaleFactor"/>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<double?> Values => NormalisedValues.Select(x => x * ScaleFactor);
    }
}
