using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class ElGamalCiphertextConverter : JsonConverter<ElGamalCiphertext>
    {
        public override ElGamalCiphertext ReadJson(JsonReader reader, Type objectType, ElGamalCiphertext existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var jObject = serializer.Deserialize<JObject>(reader);
            var publicKey = jObject["pad"].Value<string>();
            var secretKey = jObject["data"].Value<string>();

            return new ElGamalCiphertext(new ElementModP(secretKey), new ElementModP(publicKey));
        }

        public override void WriteJson(JsonWriter writer, ElGamalCiphertext value, JsonSerializer serializer)
        {
            var publicKey = value.Pad.ToHex();
            var secretKey = value.Data.ToHex();

            writer.WriteStartObject();
            writer.WritePropertyName("pad");
            writer.WriteValue(publicKey);
            writer.WritePropertyName("data");
            writer.WriteValue(secretKey);
            writer.WriteEndObject();
        }
    }
}
