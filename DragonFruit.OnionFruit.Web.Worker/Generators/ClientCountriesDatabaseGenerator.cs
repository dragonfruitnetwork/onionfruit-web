// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;
using DragonFruit.OnionFruit.Web.Worker.Storage;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public class ClientCountriesDatabaseGenerator : IDatabaseGenerator
{
    private readonly OnionooDataSource _torInfo;

    public ClientCountriesDatabaseGenerator(OnionooDataSource torInfo)
    {
        _torInfo = torInfo;
    }

    public async Task GenerateDatabase(Lazy<IDatabaseFileSink> fileSink)
    {
        // onionfruit clients accept a dictionary of key -> countrycode[]
        var clientData = new Dictionary<string, IEnumerable<string>>
        {
            ["in"] = CountriesWithFlag(_torInfo.Countries, TorNodeFlags.Guard),
            ["out"] = CountriesWithFlag(_torInfo.Countries, TorNodeFlags.Exit)
        };

        var file = fileSink.Value.CreateFile("legacy/countries.json");

        await using var writeStream = file.Open();
        await writeStream.WriteAsync(JsonSerializer.SerializeToUtf8Bytes(clientData)).ConfigureAwait(false);
    }

    private static IEnumerable<string> CountriesWithFlag(IEnumerable<IGrouping<string, TorRelayDetails>> info, TorNodeFlags flag)
    {
        return info.Where(x => x.Any(y => y.Flags.HasFlag(flag))).Select(x => x.Key);
    }
}
