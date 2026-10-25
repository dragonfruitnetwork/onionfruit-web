// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.ObjectModel;
using System.Text.Json;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Countries;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Legacy;

internal sealed class CountryListGenerator : IGenerator
{
    private const string CountriesOutputKey = "legacy/countries.json";

    public string Name => "legacy/country-list-generator";

    public IReadOnlySet<string> Sources { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        SourceOutputNames.OnionooCountries
    });

    public IReadOnlySet<string> Produces { get; } = new ReadOnlySet<string>(new HashSet<string>
    {
        CountriesOutputKey
    });

    public Task GenerateAsync(GenerationContext context, CancellationToken ct)
    {
        var countries = context.GetOutput<ICountryCapabilities>(SourceOutputNames.OnionooCountries);

        var mapping = new Dictionary<string, string[]>
        {
            ["in"] = CountriesWith(countries, RelayCapabilities.Guard),
            ["out"] = CountriesWith(countries, RelayCapabilities.Exit),
        };

        return context.Workspace.WriteAsync(CountriesOutputKey, JsonSerializer.SerializeToUtf8Bytes(mapping), ct);
    }

    private static string[] CountriesWith(ICountryCapabilities countries, RelayCapabilities capability) =>
        countries
            .Where(x => x.Value.HasFlag(capability))
            .Select(x => x.Key.ToUpperInvariant())
            .ToArray();
}