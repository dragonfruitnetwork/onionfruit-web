// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Api.Enums;
using DragonFruit.OnionFruit.Api.Objects;
using DragonFruit.OnionFruit.Api.Requests;

namespace DragonFruit.OnionFruit.Api.Extensions
{
    public static class TorStatusDetailsExtensions
    {
        public static TorStatusResponse<TorRelayDetails, TorBridgeDetails> GetTorDetails(this ApiClient client, int? limit = null, int? offset = null, TorNodeType? type = TorNodeType.Relay)
        {
            var request = new TorStatusDetailsRequest
            {
                Type = type,
                Limit = limit,
                Offset = offset
            };

            return client.Perform<TorStatusResponse<TorRelayDetails, TorBridgeDetails>>(request);
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
    }
}
