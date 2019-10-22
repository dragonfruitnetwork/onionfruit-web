using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Status.Converters;
using DragonFruit.OnionFruit.Status.Structs;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status
{
    public class Source
    {
        private const string ENDPOINT = "https://onionoo.torproject.org/details";

        [JsonProperty("version")] public double Version { get; set; }

        [JsonProperty("build_revision")] public string BuildHash { get; set; }

        [JsonProperty("relays_published")] public DateTime PublishDate { get; set; }

        [JsonProperty("relays")] public List<RelayInfo> Relays { get; set; }

        [JsonProperty("relays_truncated")] public int RelaysTruncated { get; set; }


        //leave out bridges for now - we are only interested in relays

        public static async Task<Source> GetSource(int limit = -1)
        {
            string sourceString =
                await new HttpClient().GetStringAsync($"{ENDPOINT}{(limit > 0 ? $"?limit={limit}" : string.Empty)}");

            return JsonConvert.DeserializeObject<Source>(sourceString, Json.SerializerSettings);
        }
    }
}