using Bia.Countries.Iso3166;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status.Structs
{
    public class RelayInfo
    {
        //[JsonProperty("nickname")]
        //public string Nickname { get; set; }

        //[JsonProperty("fingerprint")]
        //public string Fingerprint { get; set; }

        //[JsonProperty("running")]
        //public bool IsRunning { get; set; }

        [JsonProperty("flags")] public NodeFlags Flags { get; set; }

        [JsonProperty("country")] public string CountryCode { get; set; }

        public string CountryName
        {
            get
            {
                try
                {
                    return Countries.GetCountryByAlpha2(CountryCode.ToUpper()).ActiveDirectoryName;
                }
                catch
                {
                    return "Unknown";
                }
            }
        }

        //[JsonProperty("as")]
        //public string ASID { get; set; }

        //[JsonProperty("as_name")]
        //public string ASName { get; set; }

        [JsonProperty("observed_bandwidth")] public long Bandwidth { get; set; }

        //[JsonProperty("advertised_bandwidth")]
        //public long AdvertisedBandwidth { get; set; }

        //[JsonProperty("recommended_version")]
        //public bool RecommendedVersion { get; set; }
    }
}