// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker.Clients.Onionoo;

public class TorClientConnectionHistory
{
    [JsonPropertyName("fingerprint")]
    public string Fingerprint { get; set; }

    [JsonPropertyName("average_clients")]
    public IReadOnlyDictionary<string, TorHistoryGraph> Clients { get; set; }
}