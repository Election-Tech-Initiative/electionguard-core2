using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class SerializationSettings
    {
        public static JsonSerializerSettings NewtonsoftSettings()
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new SnakeCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            };
            settings.Converters.Add(new CiphertextBallotConverter());
            settings.Converters.Add(new CiphertextElectionContextConverter());
            settings.Converters.Add(new ElementModPConverter());
            settings.Converters.Add(new ElementModQConverter());
            settings.Converters.Add(new ElGamalKeyPairConverter());
            settings.Converters.Add(new InternalManifestConverter());
            settings.Converters.Add(new ManifestConverter());
            settings.Converters.Add(new PlaintextBallotConverter());
            return settings;
        }
    }
}
