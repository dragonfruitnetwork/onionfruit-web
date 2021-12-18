using System.Threading.Tasks;
using DragonFruit.Data;
using DragonFruit.OnionFruit.Api.Status.Enums;
using DragonFruit.OnionFruit.Api.Status.Objects;
using DragonFruit.OnionFruit.Api.Status.Requests;

namespace DragonFruit.OnionFruit.Api.Status.Extensions
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