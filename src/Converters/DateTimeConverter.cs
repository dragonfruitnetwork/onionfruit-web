// OnionFruit API/Tooling Copyright DragonFruit Network
// Licensed under the MIT License. Please refer to the LICENSE file at the root of this project for details

using System;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Api.Converters
{
    internal class DateTimeConverter : JsonConverter<DateTime>
    {
        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var date = reader.GetString();
            return DateTime.Parse(date, CultureInfo.InvariantCulture);
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            var date = value.ToString("yyyy-MM-dd hh:mm:ss");
            writer.WriteStringValue(date);
        }
    }
}
