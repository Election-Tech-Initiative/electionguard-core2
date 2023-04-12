

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class PlaintextBallotConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(PlaintextBallot);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var obj = JObject.Load(reader);
            return new PlaintextBallot(obj.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var obj = (PlaintextBallot)value;
            var json = obj.ToJson();
            writer.WriteRawValue(json);
        }
    }
}
