

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class ElementModPConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ElementModP);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new ElementModP(obj.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = (ElementModP)value;
            var json = obj.ToHex();
            writer.WriteRawValue(json);
        }
    }
}
