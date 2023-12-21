// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.Data.Serializers;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public class OnionooDataSource(ApiClient client) : IDataSource
{
    public DateTimeOffset DataLastModified { get; private set; }

    public IReadOnlyList<TorRelayDetails> Relays { get; private set; }

    public IReadOnlyCollection<IGrouping<string, TorRelayDetails>> Countries { get; private set; }

    public async Task<bool> HasDataChanged(DateTimeOffset lastVersionDate)
    {
        var request = new TorStatusRequest {LastModified = lastVersionDate};
        using var response = await client.PerformAsync(request).ConfigureAwait(false);

        switch (response.StatusCode)
        {
            case HttpStatusCode.NotModified:
                return false;

            case HttpStatusCode.OK:
                var serializer = client.Serializers.Resolve<TorStatusResponse<TorRelayDetails, TorBridgeDetails>>(DataDirection.In);

                var networkStream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false);
                var data = serializer.Deserialize<TorStatusResponse<TorRelayDetails, TorBridgeDetails>>(networkStream);

                Relays = data.Relays;
                DataLastModified = response.Content.Headers.LastModified?.UtcDateTime ?? DateTime.UtcNow;

                return true;

            default:
                // todo log issue
                return false;
        }
    }

    public async Task CollectData()
    {
        if (DataLastModified == default)
        {
            // force refresh
            await HasDataChanged(DateTimeOffset.MinValue);
        }

        // process country info
        Countries = Relays.AsParallel().Where(x => !string.IsNullOrEmpty(x.CountryCode)).GroupBy(x => x.CountryCode).ToList();
    }
}