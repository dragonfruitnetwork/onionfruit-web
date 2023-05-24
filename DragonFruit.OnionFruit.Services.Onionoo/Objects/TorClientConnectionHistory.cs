// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Services.Onionoo.Objects
{
    [Serializable, DataContract]
    [JsonObject(MemberSerialization.OptIn)]
    public class TorClientConnectionHistory
    {
        [DataMember(Name = "fingerprint")]
        public string Fingerprint { get; set; }

        [DataMember(Name = "average_clients")]
        public IReadOnlyDictionary<string, TorHistoryGraph> Clients { get; set; }
    }
}
