// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Frozen;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.CountryMapping;

public sealed class CountryNameMap
{
    private const string ResourceFileName = "country-names.json";

    private readonly IReadOnlyDictionary<string, string> _names;

    public CountryNameMap()
    {
        using var stream = OpenResourceStream();
        var file = JsonSerializer.Deserialize<CountryNamesFile>(stream) ?? throw new InvalidOperationException($"Embedded resource '{ResourceFileName}' could not be parsed.");

        _names = file.Countries.ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyDictionary<string, string> Names => _names;

    public bool TryGetValue(string countryCode, out string? name) => _names.TryGetValue(countryCode, out name);

    private static Stream OpenResourceStream()
    {
        var assembly = typeof(CountryNameMap).Assembly;
        var resourceName = $"{assembly.GetName().Name}.Sources.CountryMapping.{ResourceFileName}";
        return assembly.GetManifestResourceStream(resourceName) ?? throw new InvalidOperationException($"Embedded resource '{resourceName}' was not found in assembly '{assembly.FullName}'.");
    }
}

file sealed class CountryNamesFile
{
    [JsonPropertyName("countries")]
    public Dictionary<string, string> Countries { get; init; } = [];
}
