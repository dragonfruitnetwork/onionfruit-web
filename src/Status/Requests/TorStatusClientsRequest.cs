// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using DragonFruit.OnionFruit.Api.Status.Enums;

namespace DragonFruit.OnionFruit.Api.Status.Requests
{
    public class TorStatusClientsRequest : TorStatusRequest
    {
        public override string Stub => "clients";

        public override TorNodeType? Type => TorNodeType.Bridge;
    }
}
