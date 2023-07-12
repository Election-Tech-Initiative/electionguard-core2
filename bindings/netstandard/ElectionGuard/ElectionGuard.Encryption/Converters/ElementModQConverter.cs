using System;
using Newtonsoft.Json;

namespace ElectionGuard.Converters
{
    public class ElementModQConverter : JsonConverter<ElementModQ>
    {
        public override ElementModQ ReadJson(JsonReader reader, Type objectType, ElementModQ existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            return value != null ? new ElementModQ(value) : null;
        }

        public override void WriteJson(JsonWriter writer, ElementModQ value, JsonSerializer serializer)
        {
            var json = value.ToHex();
            writer.WriteValue(json);
        }
    }
}
