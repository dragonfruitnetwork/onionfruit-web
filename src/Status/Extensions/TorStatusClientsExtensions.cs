using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Api.Status.Objects;
using DragonFruit.OnionFruit.Api.Status.Requests;

namespace DragonFruit.OnionFruit.Api.Status.Extensions
{
    public static class TorStatusClientsExtensions
    {
        public static TorStatusResponse<TorClientConnectionHistory> GetTorBridgeConnectionMetrics(this ApiClient client, int? limit = null, int? offset = null)
        {
            var request = new TorStatusClientsRequest
            {
                Limit = limit,
                Offset = offset
            };

            return client.Perform<TorStatusResponse<TorClientConnectionHistory>>(request);
        }
        
        public static Task<TorStatusResponse<TorClientConnectionHistory>> GetTorBridgeConnectionMetricsAsync(this ApiClient client, int? limit = null, int? offset = null)
        {
            var request = new TorStatusClientsRequest
            {
                Limit = limit,
                Offset = offset
            };

            return client.PerformAsync<TorStatusResponse<TorClientConnectionHistory>>(request);
        }
    }
}