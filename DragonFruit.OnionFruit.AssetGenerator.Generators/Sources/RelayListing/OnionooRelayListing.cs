// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Text.Json;
using DragonFruit.Data;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Abstractions;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Core.Countries;
using DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing.Models;
using DragonFruit.OnionFruit.Redis.Nodes;

namespace DragonFruit.OnionFruit.AssetGenerator.Generators.Sources.RelayListing;

internal sealed class OnionooSource(ApiClient client) : ISource
{
    private static readonly IReadOnlySet<string> ProducedOutputNames = new HashSet<string>
    {
        SourceOutputNames.OnionooRelays,
        SourceOutputNames.OnionooCountries
    };

    private static readonly IReadOnlyDictionary<string, ISourceOutput> NoOutputs = new Dictionary<string, ISourceOutput>();

    private IReadOnlyDictionary<string, ISourceOutput> _outputs = NoOutputs;
    private DateTimeOffset? _dataLastModified;

    public string Name => "onionoo";

    public IReadOnlySet<string> OutputNames => ProducedOutputNames;

    public IReadOnlyDictionary<string, ISourceOutput> Outputs => _outputs;

    public async Task<bool> HasDataChangedAsync(DateTimeOffset lastVersionDate, CancellationToken ct)
    {
        using var response = await PerformRequestAsync(lastVersionDate, ct);

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotModified:
                return false;

            case HttpStatusCode.OK:
                return await PublishDataAsync(response, ct);

            default:
                return LogUnexpectedStatus(response.StatusCode);
        }
    }

    public async Task FetchAsync(CancellationToken ct)
    {
        using var response = await PerformRequestAsync(_dataLastModified, ct);

        switch (response.StatusCode)
        {
            case HttpStatusCode.OK:
                await PublishDataAsync(response, ct);
                break;

            case HttpStatusCode.NotModified:
                break;

            default:
                LogUnexpectedStatus(response.StatusCode);
                break;
        }
    }

    private Task<HttpResponseMessage> PerformRequestAsync(DateTimeOffset? ifModifiedSince, CancellationToken ct)
    {
        return client.PerformAsync(new TorStatusRequest {LastModified = ifModifiedSince}, ct);
    }

    private async Task<bool> PublishDataAsync(HttpResponseMessage response, CancellationToken ct)
    {
        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        var data = await JsonSerializer.DeserializeAsync<TorStatusResponse<TorRelayDetails, TorBridgeDetails>>(stream, cancellationToken: ct);

        var relays = data?.Relays ?? [];
        var published = response.Content.Headers.LastModified?.UtcDateTime ?? DateTime.UtcNow;

        var outputs = new Dictionary<string, ISourceOutput>(2)
        {
            [SourceOutputNames.OnionooRelays] = new TorRelayStatusOutput(published, relays),
            [SourceOutputNames.OnionooCountries] = new CountryCapabilities(relays),
        };

        Volatile.Write(ref _outputs, outputs);
        _dataLastModified = published;

        return true;
    }

    private static bool LogUnexpectedStatus(HttpStatusCode status)
    {
        Debug.WriteLine($"Onionoo details request returned an unhandled status, keeping previous data: {status}");
        return false;
    }

    private static RelayCapabilities CapabilitiesOf(TorNodeFlags flags)
    {
        var result = RelayCapabilities.None;

        if (flags.HasFlag(TorNodeFlags.Guard))
        {
            result |= RelayCapabilities.Guard;
        }

        if (flags.HasFlag(TorNodeFlags.Exit))
        {
            result |= RelayCapabilities.Exit;
        }

        return result;
    }

    private sealed class TorRelayStatusOutput : ITorRelayStatusOutput
    {
        public TorRelayStatusOutput(DateTimeOffset published, IReadOnlyList<TorRelayDetails> relays)
        {
            Published = published;
            Relays = relays;
        }

        public string Name => SourceOutputNames.OnionooRelays;

        public DateTimeOffset Published { get; }
        public IReadOnlyList<TorRelayDetails> Relays { get; }
    }

    private sealed class CountryCapabilities : ICountryCapabilities
    {
        private readonly IReadOnlyDictionary<string, RelayCapabilities> _capabilitiesByCountry;

        public CountryCapabilities(IReadOnlyList<TorRelayDetails> relays)
        {
            var mappings = new Dictionary<string, RelayCapabilities>();

            foreach (var relay in relays.Where(r => !string.IsNullOrEmpty(r.CountryCode)))
            {
                var present = CapabilitiesOf(relay.Flags);
                if (mappings.TryGetValue(relay.CountryCode, out var existing))
                {
                    mappings[relay.CountryCode] = existing | present;
                }
                else
                {
                    mappings[relay.CountryCode] = present;
                }
            }

            _capabilitiesByCountry = mappings;
        }

        public string Name => SourceOutputNames.OnionooCountries;

        public RelayCapabilities this[string key] => _capabilitiesByCountry[key];

        public IEnumerable<string> Keys => _capabilitiesByCountry.Keys;

        public IEnumerable<RelayCapabilities> Values => _capabilitiesByCountry.Values;

        public int Count => _capabilitiesByCountry.Count;

        public bool ContainsKey(string key) => _capabilitiesByCountry.ContainsKey(key);

        public bool TryGetValue(string key, [MaybeNullWhen(false)] out RelayCapabilities value)
        {
            return _capabilitiesByCountry.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<string, RelayCapabilities>> GetEnumerator() => _capabilitiesByCountry.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => _capabilitiesByCountry.GetEnumerator();
    }
}