// OnionFruit™ Web Copyright DragonFruit Network <inbox@dragonfruit.network>
// Licensed under Apache-2. Refer to the LICENSE file for more info

using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DragonFruit.OnionFruit.Web.Worker.Sources.Onionoo.Converters;

internal class MillisecondEpochConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return DateTime.UnixEpoch.AddSeconds(reader.GetInt64());
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        var epoch = (long)value.Subtract(DateTime.UnixEpoch).TotalSeconds;
        writer.WriteNumberValue(epoch);
    }
}