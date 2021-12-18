using DragonFruit.Data;

namespace DragonFruit.OnionFruit.Api.Status.Requests
{
    public class TorStatusSummaryRequest : TorStatusRequest
    {
        public override string Stub => "summary";
    }
}