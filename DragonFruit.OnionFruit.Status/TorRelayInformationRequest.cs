// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using DragonFruit.Common.Data;
using DragonFruit.Common.Data.Parameters;

namespace DragonFruit.OnionFruit.Status
{
    public class TorRelayInformationRequest : ApiRequest
    {
        public override string Path => "https://onionoo.torproject.org/details";

        public TorRelayInformationRequest()
        {
        }

        public TorRelayInformationRequest(int? limit)
        {
            Limit = limit;
        }

        [QueryParameter("search")]
        public string Query { get; set; }

        [QueryParameter("limit")]
        public int? Limit { get; set; }
    }
}
