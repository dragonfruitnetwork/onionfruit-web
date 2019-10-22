using System.Globalization;
using Newtonsoft.Json;

namespace DragonFruit.OnionFruit.Status.Converters
{
    public static class Json
    {
        /// <summary>
        ///     The default settings to be used when converting to/from json from the Tor Source
        /// </summary>
        public static JsonSerializerSettings SerializerSettings = new JsonSerializerSettings
        {
            Formatting = Formatting.Indented,
            DateTimeZoneHandling = DateTimeZoneHandling.Utc,
            Culture = new CultureInfo("en-US")
        };
    }
}