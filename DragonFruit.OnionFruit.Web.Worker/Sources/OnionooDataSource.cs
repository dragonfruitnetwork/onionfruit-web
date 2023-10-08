using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.Data.Serializers;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public class OnionooDataSource : IDataSource
{
    private readonly ApiClient _client;

    public OnionooDataSource(ApiClient client)
    {
        _client = client;
    }

    public DateTime DataLastModified { get; private set; }

    public IReadOnlyList<TorRelayDetails> Relays { get; private set; }

    public IReadOnlyCollection<IGrouping<string, TorRelayDetails>> Countries { get; private set; }

    public async Task<bool> HasDataChanged(DateTimeOffset lastVersionDate)
    {
        var request = new TorStatusDetailsRequest().Build(_client);

        // change to head (so the body isn't sent)
        request.Method = HttpMethod.Head;
        request.Headers.IfModifiedSince = lastVersionDate;

        using var response = await _client.PerformAsync(request).ConfigureAwait(false);
        return response.StatusCode != HttpStatusCode.NotModified;
    }

    public async Task CollectData()
    {
        using (var response = await _client.PerformAsync(new TorStatusDetailsRequest()).ConfigureAwait(false))
        {
            if (!response.IsSuccessStatusCode)
            {
                // todo handle failure
            }

            var serializer = _client.Serializer.Resolve<TorStatusResponse<TorRelayDetails, TorBridgeDetails>>(DataDirection.In);

            var networkStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
            var data = serializer.Deserialize<TorStatusResponse<TorRelayDetails, TorBridgeDetails>>(networkStream);

            Relays = data.Relays;
            DataLastModified = response.Content.Headers.LastModified?.UtcDateTime ?? DateTime.UtcNow;
        }

        // get country info
        Countries = Relays.AsParallel().Where(x => !string.IsNullOrEmpty(x.CountryCode)).GroupBy(x => x.CountryCode).ToList();
    }
}