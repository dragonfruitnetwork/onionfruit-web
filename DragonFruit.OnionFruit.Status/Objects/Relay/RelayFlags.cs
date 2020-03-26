// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using DragonFruit.OnionFruit.Status.Converters;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status.Objects.Relay
{
    [Flags]
    [JsonConverter(typeof(Flags))]
    public enum NodeFlags
    {
        None = 1 << 0,
        Exit = 1 << 1,
        Fast = 1 << 2,
        Guard = 1 << 3,
        Named = 1 << 4,
        Stable = 1 << 5,
        Running = 1 << 6,
        Valid = 1 << 7,
        V2Dir = 1 << 8,
        HsDir = 1 << 9
    }
}
