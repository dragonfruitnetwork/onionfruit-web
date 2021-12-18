using DragonFruit.Data;

namespace DragonFruit.OnionFruit.Api.Status.Requests
{
    public class TorStatusBandwidthRequest : TorStatusRequest
    {
        public override string Stub => "bandwidth";
    }
}