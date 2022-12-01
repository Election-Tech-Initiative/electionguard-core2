using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services
{
    public interface IKeyCeremonyService
    {
        int Create(KeyCeremony keyCeremony);
        Task<KeyCeremony> Get(int id);
        List<KeyCeremony> List();
    }

    public class KeyCeremonyService : IKeyCeremonyService
    {
        // todo: replace with database
        public static List<KeyCeremony> KeyCeremonies = new();

        public int Create(KeyCeremony keyCeremony)
        {
            keyCeremony.Id = KeyCeremonies.Count;
            KeyCeremonies.Add(keyCeremony);
            // todo: get a unique id from the database
            var keyCeremonyId = KeyCeremonies.Count - 1;
            return keyCeremonyId;
        }

        public async Task<KeyCeremony> Get(int id)
        {
            await Task.Yield();
            return KeyCeremonies[id];
        }

        public List<KeyCeremony> List()
        {
            return KeyCeremonies;
        }
    }
}
