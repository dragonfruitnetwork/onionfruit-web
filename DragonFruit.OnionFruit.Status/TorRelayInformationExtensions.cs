// OnionFruit.Status Copyright 2021 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System.Threading;
using System.Threading.Tasks;
using DragonFruit.Common.Data;

namespace DragonFruit.OnionFruit.Status
{
    public static class TorRelayInformationExtensions
    {
        public static TorRelayInformation GetServerInfo(this ApiClient client, int? limit = null, CancellationToken token = default)
        {
            return client.Perform<TorRelayInformation>(new TorRelayInformationRequest(limit), token);
        }

        public static Task<TorRelayInformation> GetServerInfoAsync(this ApiClient client, int? limit = null, CancellationToken token = default)
        {
            return client.PerformAsync<TorRelayInformation>(new TorRelayInformationRequest(limit), token);
        }
    }
}
