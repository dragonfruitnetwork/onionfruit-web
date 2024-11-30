// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using DragonFruit.OnionFruit.Web.Worker.Sources;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Enums;
using DragonFruit.OnionFruit.Web.Worker.Storage;

namespace DragonFruit.OnionFruit.Web.Worker.Generators;

public class ClientCountriesDatabaseGenerator(OnionooDataSource torInfo) : IDatabaseGenerator
{
    public async Task GenerateDatabase(FileSink fileSink)
    {
        // onionfruit clients accept a dictionary of key -> countrycode[]
        var clientData = new Dictionary<string, IEnumerable<string>>
        {
            ["in"] = CountriesWithFlag(torInfo.Countries, TorNodeFlags.Guard),
            ["out"] = CountriesWithFlag(torInfo.Countries, TorNodeFlags.Exit)
        };

        await fileSink.CreateFile("legacy/countries.json").WriteAsync(JsonSerializer.SerializeToUtf8Bytes(clientData)).ConfigureAwait(false);
    }

    private static IEnumerable<string> CountriesWithFlag(IEnumerable<IGrouping<string, TorRelayDetails>> info, TorNodeFlags flag)
    {
        return info.Where(x => x.Any(y => y.Flags.HasFlag(flag))).Select(x => x.Key.ToUpperInvariant());
    }
}