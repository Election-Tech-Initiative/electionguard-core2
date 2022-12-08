using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

public interface IKeyCeremonyService
{
    int Create(KeyCeremony keyCeremony);
    Task<KeyCeremony> Get(int id);
    Task<List<KeyCeremony>> List();
    Task<KeyCeremony?> FindByName(string keyCeremonyName);
}

public class KeyCeremonyService : IKeyCeremonyService
{
    // todo: replace with database
    private static readonly List<KeyCeremony> KeyCeremonies = new();

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

    public async Task<List<KeyCeremony>> List()
    {
        await Task.Yield();
        return KeyCeremonies;
    }

    public List<object> GetBackupPublicKeys(string guardianId)
    {
        // Get all guardian public keys.
        // Exclude current user's public key
        // 
    }

    public object ExportData()
    {
    }


    public async Task<KeyCeremony?> FindByName(string keyCeremonyName)
    {
        await Task.Yield();
        return KeyCeremonies.FirstOrDefault(i => i.Name == keyCeremonyName);
    }
}
