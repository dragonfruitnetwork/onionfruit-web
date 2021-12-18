using DragonFruit.OnionFruit.Api.Status.Enums;

namespace DragonFruit.OnionFruit.Api.Status.Requests
{
    public class TorStatusClientsRequest : TorStatusRequest
    {
        public override string Stub => "clients";

        public override TorNodeType? Type => TorNodeType.Bridge;
    }
}