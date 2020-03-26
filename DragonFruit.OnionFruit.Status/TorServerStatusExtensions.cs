// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using DragonFruit.Common.Data;
using DragonFruit.OnionFruit.Status.Objects;

namespace DragonFruit.OnionFruit.Status
{
    public static class TorServerStatusExtensions
    {
        public static TorServerStatusResponse GetServerInfo(this ApiClient client) =>
            GetServerInfo(client, null);

        public static TorServerStatusResponse GetServerInfo(this ApiClient client, int? limit) =>
            client.Perform<TorServerStatusResponse>(new TorServerStatusRequest(limit));
    }
}
