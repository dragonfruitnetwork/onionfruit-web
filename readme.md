# OnionFruit™ API
![CI Publish](https://github.com/dragonfruitnetwork/onionfruit-api/workflows/Publish/badge.svg)
![CI Unit Tests](https://github.com/dragonfruitnetwork/onionfruit-api/workflows/Unit%20Tests/badge.svg)
[![Nuget](https://img.shields.io/nuget/v/DragonFruit.OnionFruit.Api)](https://nuget.org/packages/DragonFruit.OnionFruit.Api)
![Nuget Downloads](https://img.shields.io/nuget/dt/DragonFruit.OnionFruit.Api)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
The OnionFruit™ API provides developers with an easy way to get access to metrics from the Tor service, including graph data and network overviews.

### Usage

1. Install the NuGet package (see icons above)
2. Create an `ApiClient` for the lifetime of the program
3. Use one of the [extension methods](/tree/master/src/Extensions), or create your own request/response combo
> The response objects in this project are mapped to `System.Text.Json` serializers. These will be changed to `DataMember` in the future once the .NET team add support

```cs
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.Data.Serializers.SystemJson;
using DragonFruit.OnionFruit.Api.Requests;
using DragonFruit.OnionFruit.Api.Extensions;

namespace OnionFruit.Demo;

static readonly ApiClient _client = new ApiClient<ApiSystemTextJson>();

public static async Task Main(string[] args)
{
    // get the first 500 nodes in the tor directory
    var data = _client.GetTorDetails(500);
    
    // or asynchronously
    var asyncData = await _client.GetTorDetailsAsync(500);
    
    // and if the basic usage is not enough, create the request and perform it manually:
    var customRequest = new TorStatusBandwidthRequest
    {
        CountryCode = "US"
    };
    
    // check the extesions for the response type
    var customResponse = await _client.PerformAsync<TorStatusResponse<TorNodeBandwidthHistory>>(customRequest);
}
```

### License
These (the api and tooling) are open-source components of OnionFruit™. These are licensed under the MIT license.
Refer to the license file for more info.