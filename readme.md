# OnionFruit™ Web
[![Nuget](https://img.shields.io/nuget/v/DragonFruit.OnionFruit.Api)](https://nuget.org/packages/DragonFruit.OnionFruit.Api)
![Nuget Downloads](https://img.shields.io/nuget/dt/DragonFruit.OnionFruit.Api)
[![DragonFruit Discord](https://img.shields.io/discord/482528405292843018?label=Discord&style=popout)](https://discord.gg/VA26u5Z)

## Overview
This repo contains a collection of libraries developed to work with the OnionFruit clients, more specifically to retrieve data used for country selection and IP filtering.

### OnionFruit™ Onionoo API
An easy way to access metrics from the Tor service, including graph data and network overviews.

#### Usage

1. Install the NuGet package
2. Create an `ApiClient` for the lifetime of the program
3. Use one of the [extension methods](/DragonFruit.OnionFruit.Web.Onionoo/TorWebExtensions.cs), or create your own request/response combo

```cs
using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.Data.Serializers.Newtonsoft;
using DragonFruit.OnionFruit.Services.Onionoo;
using DragonFruit.OnionFruit.Services.Onionoo.Requests;

namespace OnionFruit.Demo;

static readonly ApiClient _client = new ApiClient<ApiJsonSerializer>();

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
    
    // check the extension methods' return types to determine the type to pass to the client
    var customResponse = await _client.PerformAsync<TorStatusResponse<TorNodeBandwidthHistory>>(customRequest);
}
```

### License

These libraries and components are licensed under the MIT license. Refer to the license file for more info.
