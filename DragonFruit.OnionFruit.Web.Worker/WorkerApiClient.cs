// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Net;
using System.Net.Http;
using DragonFruit.Data;
using DragonFruit.Data.Serializers.SystemJson;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Converters;

namespace DragonFruit.OnionFruit.Web.Worker;

public class WorkerApiClient : ApiClient<ApiSystemTextJsonSerializer>
{
    public WorkerApiClient()
    {
        UserAgent = $"OnionFruit-Web-Worker/{GetType().Assembly.GetName().Version?.ToString(3)}";
        Handler = () => new SocketsHttpHandler
        {
            UseCookies = false,
            AutomaticDecompression = DecompressionMethods.All
        };

        Serializer.Configure<ApiSystemTextJsonSerializer>(json => json.SerializerOptions.Converters.Add(new DateTimeConverter()));
    }
}