// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker
{
    public class CountryMap
    {
        static CountryMap()
        {
            using var mapping = typeof(CountryMap).Assembly.GetManifestResourceStream($"{typeof(CountryMap).Assembly.FullName}.country-descriptors.json");
            Instance = JsonSerializer.Deserialize<CountryMap>(mapping);
        }

        [JsonIgnore]
        public static CountryMap Instance { get; }

        /// <summary>
        /// Gets the operating system info the mapping was generated on
        /// </summary>
        [JsonPropertyName("generated_on")]
        public string GeneratedOn { get; set; }

        /// <summary>
        /// The mapping of country codes to country names
        /// </summary>
        [JsonInclude, JsonPropertyName("countries")]
        private IReadOnlyDictionary<string, string> CodeMap { get; set; }

        /// <summary>
        /// Gets the country name associated with the provided <see cref="code"/>, or <c>null</c> if not found.
        /// </summary>
        public string GetCountryName(string code) => CodeMap.GetValueOrDefault(code);
    }
}