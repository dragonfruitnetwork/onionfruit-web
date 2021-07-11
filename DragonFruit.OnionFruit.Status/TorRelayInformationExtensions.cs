// OnionFruit.Status Copyright 2021 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System.Threading;
using DragonFruit.Common.Data;

namespace DragonFruit.OnionFruit.Status
{
    public static class TorRelayInformationExtensions
    {
        public static TorRelayInformation GetServerInfo(this ApiClient client, CancellationToken token = default)
        {
            return GetServerInfo(client, null, token);
        }

        public static TorRelayInformation GetServerInfo(this ApiClient client, int? limit, CancellationToken token = default)
        {
            return client.Perform<TorRelayInformation>(new TorRelayInformationRequest(limit), token);
        }
    }
}
