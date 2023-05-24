// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using DragonFruit.Data;
using DragonFruit.Data.Parameters;
using DragonFruit.OnionFruit.Services.Onionoo.Enums;

namespace DragonFruit.OnionFruit.Services.Onionoo.Requests
{
    public abstract class TorStatusRequest : ApiRequest
    {
        public override string Path => $"https://onionoo.torproject.org/{Stub}";

        public abstract string Stub { get; }

        /// <summary>
        /// Query to filter nodes by
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_search
        /// </remarks>
        [QueryParameter("search")]
        public string Query { get; set; }

        /// <summary>
        /// Filter nodes by hostname
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_host_name
        /// </remarks>
        [QueryParameter("host_name")]
        public string Hostname { get; set; }

        /// <summary>
        /// When set, filters nodes by the currently recommended version (as defined by Tor)
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_recommended_version
        /// </remarks>
        [QueryParameter("recommended_version")]
        public bool? FilterRecommendedVersion { get; set; }

        /// <summary>
        /// Optional country code to filter nodes by
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_country
        /// </remarks>
        [QueryParameter("country")]
        public string CountryCode { get; set; }

        /// <summary>
        /// Optional operating system filter
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_os
        /// </remarks>
        [QueryParameter("os")]
        public string OperatingSystem { get; set; }

        /// <summary>
        /// When set, filters out nodes by their current uptime status
        /// </summary>
        [QueryParameter("running")]
        public bool? Running { get; set; }

        /// <summary>
        /// Optional offset
        /// </summary>
        [QueryParameter("offset")]
        public int? Offset { get; set; }

        /// <summary>
        /// Optional limit
        /// </summary>
        [QueryParameter("limit")]
        public int? Limit { get; set; }

        /// <summary>
        /// Defines what type of server should be returned
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_type
        /// </remarks>
        [QueryParameter("type", EnumHandlingMode.StringLower)]
        public virtual TorNodeType? Type { get; set; }

        /// <summary>
        /// Reorder the response data (defaults to ascending order).
        /// To order by descending order, prefix the field with a minus symbol (-)
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_order
        /// </remarks>
        [QueryParameter("order", CollectionConversionMode.Concatenated)]
        public IEnumerable<string> Order { get; set; }

        /// <summary>
        /// The specific fields to return. Defaults to return everything
        /// </summary>
        /// <remarks>
        /// https://metrics.torproject.org/onionoo.html#parameters_fields
        /// </remarks>
        [QueryParameter("fields", CollectionConversionMode.Concatenated)]
        public IEnumerable<string> ResponseFields { get; set; }
    }
}
