// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Frozen;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker
{
    public class CountryMap : IJsonOnDeserialized
    {
        static CountryMap()
        {
            using var mapping = typeof(CountryMap).Assembly.GetManifestResourceStream($"{typeof(CountryMap).Assembly.GetName().Name}.country-descriptors.json");
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
        [JsonPropertyName("countries")]
        public IReadOnlyDictionary<string, string> SourceMap { get; set; }

        /// <summary>
        /// Gets the country name associated with the provided <see cref="code"/>, or <c>null</c> if not found.
        /// </summary>
        public string GetCountryName(string code) => SourceMap.GetValueOrDefault(code.ToUpperInvariant());

        void IJsonOnDeserialized.OnDeserialized()
        {
            // convert the dictionary to a frozen dictionary if it's not already
            if (SourceMap.GetType().GetGenericTypeDefinition() != typeof(FrozenDictionary<,>))
            {
                SourceMap = SourceMap.ToFrozenDictionary();
            }
        }
    }
}