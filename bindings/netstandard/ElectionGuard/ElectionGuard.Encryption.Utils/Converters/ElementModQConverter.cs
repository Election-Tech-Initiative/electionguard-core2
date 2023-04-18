

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class ElementModQConverter : JsonConverter<ElementModQ>
    {
        public override ElementModQ ReadJson(JsonReader reader, Type objectType, ElementModQ existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var value = (string)reader.Value;
            return new ElementModQ(value);
        }

        public override void WriteJson(JsonWriter writer, ElementModQ value, JsonSerializer serializer)
        {
            var json = value.ToHex();
            writer.WriteValue(json);
        }
    }
}
