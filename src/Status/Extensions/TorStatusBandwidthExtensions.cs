// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Api.Status.Enums;
using DragonFruit.OnionFruit.Api.Status.Objects;
using DragonFruit.OnionFruit.Api.Status.Requests;

namespace DragonFruit.OnionFruit.Api.Status.Extensions
{
    public static class TorStatusBandwidthExtensions
    {
        public static TorStatusResponse<TorNodeBandwidthHistory> GetBandwidthHistory(this ApiClient client, int? limit = null, int? offset = null, TorNodeType? type = TorNodeType.Relay)
        {
            var request = new TorStatusBandwidthRequest
            {
                Type = type,
                Limit = limit,
                Offset = offset
            };

            return client.Perform<TorStatusResponse<TorNodeBandwidthHistory>>(request);
        }

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
    }
}
