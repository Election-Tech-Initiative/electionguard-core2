using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

public interface IKeyCeremonyService
{
    string Create(KeyCeremony keyCeremony);
    Task<KeyCeremony> Get(int id);
    Task<List<KeyCeremony>> List();
    Task<KeyCeremony?> FindByName(string keyCeremonyName);
}

public class KeyCeremonyService : IKeyCeremonyService
{
    // todo: replace with database
    private static readonly List<KeyCeremony> KeyCeremonies = new();

    public string Create(KeyCeremony keyCeremony)
    {
        KeyCeremonies.Add(keyCeremony);
        // todo: get a unique id from the database
        var keyCeremonyId = KeyCeremonies.Count - 1;
        return keyCeremony.Id;
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
        return new();
    }

    public object ExportData()
    {
        return new();
    }


    public async Task<KeyCeremony?> FindByName(string keyCeremonyName)
    {
        await Task.Yield();
        return KeyCeremonies.FirstOrDefault(i => i.Name == keyCeremonyName);
    }
}
