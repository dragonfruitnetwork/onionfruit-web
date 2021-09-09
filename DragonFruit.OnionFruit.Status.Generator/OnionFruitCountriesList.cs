// OnionFruit.Status Copyright 2021 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace DragonFruit.OnionFruit.Status.Generator
{
    [Serializable]
    [DataContract]
    public class OnionFruitCountriesList
    {
        [DataMember(Name = "in")]
        public string[] In { get; set; }

        [DataMember(Name = "out")]
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
