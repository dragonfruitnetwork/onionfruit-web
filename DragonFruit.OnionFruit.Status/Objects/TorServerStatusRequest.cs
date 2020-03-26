// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using DragonFruit.Common.Data;
using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.OnionFruit.Status.Objects
{
    public class TorServerStatusRequest : ApiRequest
    {
        public override string Path => "https://onionoo.torproject.org/details";

        public TorServerStatusRequest()
        {
        }

        public TorServerStatusRequest(int? limit)
        {
            Limit = limit;
        }

        [QueryParameter("limit")]
        public int? Limit { get; set; }
    }
}
