

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class InternalManifestConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(InternalManifest);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new InternalManifest(obj.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = (InternalManifest)value;
            var json = obj.ToJson();
            writer.WriteRawValue(json);
        }
    }
}
