// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo;

public class TorClientConnectionHistory
{
    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; set; }

    [JsonPropertyName("average_clients")]
    public IReadOnlyDictionary<string, TorHistoryGraph> Clients { get; set; }
}