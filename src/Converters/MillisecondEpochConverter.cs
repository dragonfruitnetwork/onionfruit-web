// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Api.Converters
{
    internal class MillisecondEpochConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var time = reader.GetInt64();
            return new DateTime(1970, 1, 1).AddMilliseconds(time);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var epoch = value.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
            writer.WriteNumberValue((int)epoch);
        }
    }
}
