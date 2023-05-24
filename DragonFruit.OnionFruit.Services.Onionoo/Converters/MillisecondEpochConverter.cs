// OnionFruit Web Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Services.Onionoo.Converters
{
    internal class MillisecondEpochConverter : JsonConverter<DateTime>
    {
        public override void WriteJson(JsonWriter writer, DateTime value, JsonSerializer serializer)
        {
            var epoch = value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            writer.WriteValue((int)epoch);
        }

        public override DateTime ReadJson(JsonReader reader, Type objectType, DateTime existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var time = reader.ReadAsString();
            return new DateTime(1970, 1, 1).AddMilliseconds(long.Parse(time));
        }
    }
}
