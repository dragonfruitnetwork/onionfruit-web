// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Bia.Countries.Iso3166;
using DragonFruit.Data;
using DragonFruit.Data.Serializers.SystemJson;
using DragonFruit.OnionFruit.Api.Status.Enums;
using DragonFruit.OnionFruit.Api.Status.Extensions;
using DragonFruit.OnionFruit.Api.Status.Objects;

namespace DragonFruit.OnionFruit.CountryListGenerator
{
    internal static class Program
    {
        private static readonly ApiClient Client = new ApiClient<ApiSystemTextJsonSerializer>();

        private static async Task Main(string[] args)
        {
            Console.WriteLine("Fetching data...");
            var data = await Client.GetTorDetailsAsync(type: TorNodeType.Relay);
            var countries = data.Relays.GroupBy(x => x.CountryCode?.ToUpper()).Where(x => Countries.GetCountryByAlpha2(x.Key) is not null).ToArray();

            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"Downloaded {data.Relays.Length} relay metadata packets over {countries.Length} countries");

            // onionfruit clients accept a dictionary of key -> countrycode[]
            var clientData = new Dictionary<string, string[]>
            {
                ["in"] = countries.GetCountriesWithFlag(TorNodeFlags.Guard).ToArray(),
                ["out"] = countries.GetCountriesWithFlag(TorNodeFlags.Exit).ToArray()
            };

            Console.WriteLine($"Response contains {clientData["in"].Length} entry countries and {clientData["out"].Length} exit countries");

            var saveLocation = Environment.GetEnvironmentVariable("OUTPUT_FILE") ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "countries.json");
            Console.Write($"Writing to {saveLocation}...");

            FileServices.WriteFile(saveLocation, clientData);

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("Complete");
        }

        private static IEnumerable<string> GetCountriesWithFlag(this IEnumerable<IGrouping<string, TorRelayDetails>> info, TorNodeFlags flag)
        {
            return info.Where(x => x.Any(y => y.Flags.HasFlag(flag))).Select(x => x.Key);
        }
    }
}
