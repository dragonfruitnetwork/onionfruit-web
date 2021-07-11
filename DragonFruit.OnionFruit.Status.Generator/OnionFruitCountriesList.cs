// OnionFruit.Status Copyright 2021 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status.Generator
{
    public class OnionFruitCountriesList
    {
        [JsonProperty("in")]
        public string[] In { get; set; }

        [JsonProperty("out")]
        public string[] Out { get; set; }

        private static IEnumerable<string> GetCountriesWithFlag(IEnumerable<IGrouping<string, TorRelay>> info, RelayFlags flag) =>
            info.Where(x => x.Any(y => y.Flags.HasFlag(flag))).Select(x => x.Key);

        public static explicit operator OnionFruitCountriesList(IGrouping<string, TorRelay>[] info) => new()
        {
            In = GetCountriesWithFlag(info, RelayFlags.Guard).ToArray(),
            Out = GetCountriesWithFlag(info, RelayFlags.Exit).ToArray()
        };
    }
}
