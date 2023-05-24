// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Services.Onionoo.Enums;
using DragonFruit.OnionFruit.Services.Onionoo.Objects;
using DragonFruit.OnionFruit.Services.Onionoo.Requests;

namespace DragonFruit.OnionFruit.Services.Onionoo
{
    public static class TorWebExtensions
    {
        public static Task<TorStatusResponse<TorNodeBandwidthHistory>> GetBandwidthHistoryAsync(this ApiClient client, int? limit = null, int? offset = null, TorNodeType? type = TorNodeType.Relay)
        {
            var request = new TorStatusBandwidthRequest
            {
                Type = type,
                Limit = limit,
                Offset = offset
            };

            return client.PerformAsync<TorStatusResponse<TorNodeBandwidthHistory>>(request);
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

        public static Task<TorStatusResponse<TorRelayDetails, TorBridgeDetails>> GetTorDetailsAsync(this ApiClient client, int? limit = null, int? offset = null, TorNodeType? type = TorNodeType.Relay)
        {
            var request = new TorStatusDetailsRequest
            {
                Type = type,
                Limit = limit,
                Offset = offset
            };

            return client.PerformAsync<TorStatusResponse<TorRelayDetails, TorBridgeDetails>>(request);
        }

        public static Task<TorStatusResponse<TorRelaySummary, TorBridgeSummary>> GetTorSummaryAsync(this ApiClient client, int? limit = null, int? offset = null, TorNodeType? type = TorNodeType.Relay)
        {
            var request = new TorStatusSummaryRequest
            {
                Type = type,
                Limit = limit,
                Offset = offset
            };

            return client.PerformAsync<TorStatusResponse<TorRelaySummary, TorBridgeSummary>>(request);
        }
    }
}
