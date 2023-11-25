// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;
using DragonFruit.OnionFruit.Web.Worker.Storage.Abstractions;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public class ClientCountriesDatabaseGenerator : IDatabaseGenerator
{
    private readonly OnionooDataSource _torInfo;

    public ClientCountriesDatabaseGenerator(OnionooDataSource torInfo)
    {
        _torInfo = torInfo;
    }

    public async Task GenerateDatabase(IFileSink fileSink)
    {
        // onionfruit clients accept a dictionary of key -> countrycode[]
        var clientData = new Dictionary<string, IEnumerable<string>>
        {
            ["in"] = CountriesWithFlag(_torInfo.Countries, TorNodeFlags.Guard),
            ["out"] = CountriesWithFlag(_torInfo.Countries, TorNodeFlags.Exit)
        };

        await fileSink.CreateFile("legacy/countries.json").WriteAsync(JsonSerializer.SerializeToUtf8Bytes(clientData)).ConfigureAwait(false);
    }

    private static IEnumerable<string> CountriesWithFlag(IEnumerable<IGrouping<string, TorRelayDetails>> info, TorNodeFlags flag)
    {
        return info.Where(x => x.Any(y => y.Flags.HasFlag(flag))).Select(x => x.Key.ToUpperInvariant());
    }
}