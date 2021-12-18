using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Api.Status.Enums;
using DragonFruit.OnionFruit.Api.Status.Objects;
using DragonFruit.OnionFruit.Api.Status.Requests;

namespace DragonFruit.OnionFruit.Api.Status.Extensions
{
    public static class TorStatusSummaryExtensions
    {
        public static TorStatusResponse<TorRelaySummary, TorBridgeSummary> GetTorSummary(this ApiClient client, int? limit = null, int? offset = null, TorNodeType? type = TorNodeType.Relay)
        {
            var request = new TorStatusSummaryRequest
            {                
                Type = type,
                Limit = limit,
                Offset = offset
            };

            return client.Perform<TorStatusResponse<TorRelaySummary, TorBridgeSummary>>(request);
        }

        public static Task<TorStatusResponse<TorRelaySummary, TorBridgeSummary>> GetTorSummaryAsync(this ApiClient client, int? limit = null, int? offset = null, TorNodeType? type = TorNodeType.Relay)
        {
            var request = new TorStatusSummaryRequest
            {
                Type = type,
                Limit = limit,
                Offset = offset,
            };

            return client.PerformAsync<TorStatusResponse<TorRelaySummary, TorBridgeSummary>>(request);
        }
    }
}