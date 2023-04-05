using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using System.Text.Json;

namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Guardian storage extensions for working with local storage
/// </summary>
public static class GuardianStorageExtensions
{
    internal const string GuardianPrefix = "guardian_";
    internal const string PrivateKeyFolder = "gui_private_keys";
    internal const string GuardianExt = ".json";

    public static Guardian FromPrivateRecord(
        GuardianPrivateRecord privateGuardianRecord,
        string keyCeremonyId,
        int numberOfGuardians,
        int quorum)
    {
        return new(
            privateGuardianRecord.ElectionKeys,
            new(keyCeremonyId, numberOfGuardians, quorum),
            privateGuardianRecord.GuardianElectionPublicKeys,
            privateGuardianRecord.GuardianElectionPartialKeyBackups,
            privateGuardianRecord.BackupsToShare,
            privateGuardianRecord.GuardianElectionPartialKeyVerifications);
    }

    /// <summary>
    /// Loads the guardian from local storage device
    /// </summary>
    /// <param name="guardianId">guardian id</param>
    /// <param name="keyCeremonyId">id for the key ceremony</param>
    /// <param name="guardianCount">count of guardians</param>
    /// <param name="quorum">minimum needed number of guardians</param>
    /// <returns></returns>
    public static Guardian? Load(string guardianId, string keyCeremonyId, int guardianCount, int quorum)
    {
        var storage = StorageService.GetInstance();

        var filename = GuardianPrefix + guardianId + GuardianExt;
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(basePath, PrivateKeyFolder, keyCeremonyId, filename);

        var data = storage.FromFile(filePath);
        try
        {
            var privateGuardian = JsonSerializer.Deserialize<GuardianPrivateRecord>(data);
            return privateGuardian != null ?
                FromPrivateRecord(privateGuardian, keyCeremonyId, guardianCount, quorum) :
                null;
        }
        catch (Exception ex)
        {
            throw new ElectionGuardException("Could not load guardian", ex);
        }
    }

    /// <summary>
    /// Loads the guardian from local storage device
    /// </summary>
    /// <param name="guardianId">guardian id</param>
    /// <param name="keyCeremony">key ceremony record</param>
    public static Guardian? Load(string guardianId, KeyCeremonyRecord keyCeremony)
    {
        return Load(guardianId, keyCeremony.KeyCeremonyId!, keyCeremony.NumberOfGuardians, keyCeremony.Quorum);
    }

    /// <summary>
    /// Saves the guardian to the local storage device
    /// </summary>
    public static void Save(this Guardian self, string keyCeremonyId)
    {
        var storage = StorageService.GetInstance();

        GuardianPrivateRecord data = self;
        var dataJson = JsonSerializer.Serialize(data);

        var filename = GuardianPrefix + data.GuardianId + GuardianExt;
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(basePath, PrivateKeyFolder, keyCeremonyId);

        storage.ToFile(filePath, filename, dataJson);
    }
}
