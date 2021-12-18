// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using DragonFruit.OnionFruit.Api.Converters;

namespace DragonFruit.OnionFruit.Api.Objects
{
    [Serializable]
    public class TorHistoryGraph
    {
        [JsonPropertyName("first")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime Start { get; set; }

        [JsonPropertyName("last")]
        [JsonConverter(typeof(DateTimeConverter))]
        public DateTime End { get; set; }

        [JsonPropertyName("interval")]
        public int IntervalSeconds { get; set; }

        /// <summary>
        /// The <see cref="TimeSpan"/> between two data points
        /// </summary>
        [JsonIgnore]
        public TimeSpan Interval => TimeSpan.FromSeconds(IntervalSeconds);

        /// <summary>
        /// The scale factor each value needs to be multiplied by to get the original value
        /// </summary>
        [JsonPropertyName("factor")]
        public double ScaleFactor { get; set; }

        /// <summary>
        /// The normalised values, as sent by the api
        /// </summary>
        [JsonPropertyName("values")]
        public int?[] NormalisedValues { get; set; }

        /// <summary>
        /// The values, multiplied out by the <see cref="ScaleFactor"/>
        /// </summary>
        [JsonIgnore]
        public IEnumerable<double?> Values => NormalisedValues.Select(x => x * ScaleFactor);
    }
}
