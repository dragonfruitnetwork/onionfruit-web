using DragonFruit.Data;
using DragonFruit.Data.Serializers.SystemJson;

namespace DragonFruit.OnionFruit.Web.Worker.Clients
{
    public class WorkerClient : ApiClient<ApiSystemTextJsonSerializer>
    {
        public WorkerClient()
        {
            UserAgent = $"OnionFruit-Web-Worker/{GetType().Assembly.GetName().Version?.ToString(3)}";
        }
    }
}