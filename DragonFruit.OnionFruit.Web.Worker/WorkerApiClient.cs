using DragonFruit.Data;
using DragonFruit.Data.Serializers.SystemJson;
using DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Converters;

namespace DragonFruit.OnionFruit.Web.Worker;

public class WorkerApiClient : ApiClient<ApiSystemTextJsonSerializer>
{
    public WorkerApiClient()
    {
        UserAgent = $"OnionFruit-Web-Worker/{GetType().Assembly.GetName().Version?.ToString(3)}";
        Serializer.Configure<ApiSystemTextJsonSerializer>(json => json.SerializerOptions.Converters.Add(new DateTimeConverter()));
    }
}