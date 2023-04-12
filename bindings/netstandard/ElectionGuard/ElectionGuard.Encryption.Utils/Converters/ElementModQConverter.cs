

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class ElementModQConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(ElementModQ);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new ElementModQ(obj.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = (ElementModQ)value;
            var json = obj.ToHex();
            writer.WriteValue(json);
        }
    }
}
