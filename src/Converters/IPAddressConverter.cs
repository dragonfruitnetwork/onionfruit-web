using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Api.Converters
{
    internal class IPAddressConverter : JsonConverter<IPAddress>
    {
        public override IPAddress Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var ip = reader.GetString();
            return IPAddress.Parse(ip);
        }

        public override void Write(Utf8JsonWriter writer, IPAddress value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value.ToString());
        }
    }
}