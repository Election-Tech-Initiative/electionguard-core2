using Newtonsoft.Json;

namespace ElectionGuard.Encryption.Utils.Converters
{
    public class SerializationSettings
    {
        public static JsonSerializerSettings NewtonsoftSettings()
        {
            var settings = new JsonSerializerSettings();
            settings.Converters.Add(new CiphertextBallotConverter());
            settings.Converters.Add(new ElementModPConverter());
            settings.Converters.Add(new ElementModQConverter());
            return settings;
        }
    }
}
