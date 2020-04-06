// OnionFruit.Status Copyright 2020 DragonFruit Network <inbox@dragonfruit.network>
// Licensed under MIT. Please refer to the LICENSE file for more info

using System;
using System.Collections.Generic;
using System.Linq;
using DragonFruit.OnionFruit.Status.Objects.Relay;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DragonFruit.OnionFruit.Status.Converters
{
    public class Flags : JsonConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            NodeFlags currentFlags = NodeFlags.None;
            var rawFlags = JToken.Load(reader).ToObject<List<string>>();

            //check for flags and set enum
            if (rawFlags.Contains("Fast")) currentFlags |= NodeFlags.Fast;
            if (rawFlags.Contains("Guard")) currentFlags |= NodeFlags.Guard;
            if (rawFlags.Contains("Running")) currentFlags |= NodeFlags.Running;
            if (rawFlags.Contains("Stable")) currentFlags |= NodeFlags.Stable;
            if (rawFlags.Contains("Valid")) currentFlags |= NodeFlags.Valid;
            if (rawFlags.Contains("Named")) currentFlags |= NodeFlags.Named;
            if (rawFlags.Contains("Exit")) currentFlags |= NodeFlags.Exit;
            if (rawFlags.Contains("HSDir")) currentFlags |= NodeFlags.HsDir;
            if (rawFlags.Contains("V2Dir")) currentFlags |= NodeFlags.V2Dir;

            if (currentFlags > NodeFlags.None)
                currentFlags ^= NodeFlags.None; //toggle none if there are others selected

            return currentFlags;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var flags = value.ToString()
                             .Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries)
                             .Select(f => $"\"{f}\"");

            writer.WriteRawValue($"[{string.Join(", ", flags)}]");
        }

        public override bool CanConvert(Type objectType) => true;
    }
}
