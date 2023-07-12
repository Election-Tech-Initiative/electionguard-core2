using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElectionGuard.Converters
{
    public class ElGamalKeyPairConverter : JsonConverter<ElGamalKeyPair>
    {
        public override ElGamalKeyPair ReadJson(JsonReader reader, Type objectType, ElGamalKeyPair existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = serializer.Deserialize<JObject>(reader);
            var publicKey = jObject["public_key"].Value<string>();
            var secretKey = jObject["secret_key"].Value<string>();

            return new ElGamalKeyPair(new ElementModQ(secretKey), new ElementModP(publicKey));
        }

        public override void WriteJson(JsonWriter writer, ElGamalKeyPair value, JsonSerializer serializer)
        {
            var publicKey = value.PublicKey.ToHex();
            var secretKey = value.SecretKey.ToHex();

            writer.WriteStartObject();
            writer.WritePropertyName("public_key");
            writer.WriteValue(publicKey);
            writer.WritePropertyName("secret_key");
            writer.WriteValue(secretKey);
            writer.WriteEndObject();
        }
    }
}
