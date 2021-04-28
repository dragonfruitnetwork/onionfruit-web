// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status
{
    [Serializable]
    [JsonObject(MemberSerialization.OptIn)]
    public class TorRelay
    {
        private RelayFlags? _flags;

        /// <summary>
        /// Server nickname
        /// </summary>
        [JsonProperty("nickname")]
        public string Nickname { get; set; }

        /// <summary>
        /// Server fingerprint
        /// </summary>
        [JsonProperty("fingerprint")]
        public string Fingerprint { get; set; }

        [JsonProperty("first_seen")]
        public DateTimeOffset FirstSeen { get; set; }

        [JsonProperty("last_seen")]
        public DateTimeOffset LastSeen { get; set; }

        /// <summary>
        /// Whether the server is currently online
        /// </summary>
        [JsonProperty("running")]
        public bool Running { get; set; }

        /// <summary>
        /// ISO 3166-1 alpha-2 country code
        /// </summary>
        [JsonProperty("country")]
        public string CountryCode { get; set; }

        /// <summary>
        /// The country name, in English
        /// </summary>
        [JsonProperty("country_name")]
        public string CountryName { get; set; }

        /// <summary>
        /// The name of the region the server is located
        /// </summary>
        [JsonProperty("region_name")]
        public string RegionName { get; set; }

        /// <summary>
        /// The name of the city the server is in
        /// </summary>
        [JsonProperty("city_name")]
        public string CityName { get; set; }

        /// <summary>
        /// The latitude of the server
        /// </summary>
        [JsonProperty("latitude")]
        public double Latitude { get; set; }

        /// <summary>
        /// The longitude of the server
        /// </summary>
        [JsonProperty("longitude")]
        public double Longitude { get; set; }

        /// <summary>
        /// The ASN of the service provider
        /// </summary>
        [JsonProperty("as")]
        public string AS { get; set; }

        /// <summary>
        /// The formatted ASN of the internet service provider (company name)
        /// </summary>
        [JsonProperty("as_name")]
        public string ASName { get; set; }

        /// <summary>
        /// The bandwidth, in bytes, of the current server
        /// </summary>
        [JsonProperty("observed_bandwidth")]
        public long Bandwidth { get; set; }

        /// <summary>
        /// Contact information for the server owner
        /// </summary>
        [JsonProperty("contact")]
        public string Contact { get; set; }

        /// <summary>
        /// The operating system the tor software is running on
        /// </summary>
        [JsonProperty("platform")]
        public string Platform { get; set; }

        /// <summary>
        /// The software version
        /// </summary>
        [JsonProperty("version")]
        public string Version { get; set; }

        /// <summary>
        /// Directory address
        /// </summary>
        [JsonProperty("dir_address")]
        public string DirAddress { get; set; }

        /// <summary>
        /// Collection of all exit IPs
        /// </summary>
        [JsonProperty("exit_addresses")]
        public string[] ExitAddresses { get; set; }

        [JsonProperty("or_addresses")]
        public string[] OrAddresses { get; set; }

        [JsonProperty("flags")]
        public IEnumerable<string> FlagsRaw { get; set; }

        public RelayFlags Flags => _flags ??= (FlagsRaw ?? Enumerable.Empty<string>()).Aggregate(RelayFlags.None, (current, flag) =>
        {
            // ReSharper disable once ConvertToLambdaExpression (the line is too long otherwise)
            return Enum.TryParse(flag, out RelayFlags parsedFlag) ? current | parsedFlag : current;
        });
    }
}
