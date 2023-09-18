using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.Data.Extensions;
using DragonFruit.OnionFruit.Web.Worker.Clients.Onionoo;

namespace DragonFruit.OnionFruit.Web.Worker.Sources;

public class TorDirectoryInfoSource : IDataSource
{
    private readonly ApiClient _client;

    public TorDirectoryInfoSource(ApiClient client)
    {
        _client = client;
    }
    
    public IReadOnlyList<TorRelayDetails> Relays { get; private set; }
    
    public async Task<bool> HasDataChanged(DateTime lastVersionDate)
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
        var data = await _client.PerformAsync<TorStatusResponse<TorRelayDetails>>(new TorStatusDetailsRequest()).ConfigureAwait(false);
        
        Relays = data.Relays;
    }
}