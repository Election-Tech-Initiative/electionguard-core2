using ElectionGuard.Converters;
using ElectionGuard.Guardians;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;

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
        int quorum,
        Dictionary<string, ElectionPublicKey>? publicKeys = null,
        Dictionary<string, ElectionPartialKeyBackup>? otherBackups = null)
    {
        return new(
            privateGuardianRecord.ElectionKeys,
            privateGuardianRecord.CommitmentSeed,
            new(keyCeremonyId, numberOfGuardians, quorum),
            publicKeys,
            otherBackups);
    }

    /// <summary>
    /// Loads the guardian from local storage device
    /// </summary>
    /// <param name="guardianId">guardian id</param>
    /// <param name="keyCeremonyId">id for the key ceremony</param>
    /// <param name="guardianCount">count of guardians</param>
    /// <param name="quorum">minimum needed number of guardians</param>
    /// <returns></returns>
    public static Guardian? Load(
        string guardianId,
        string keyCeremonyId,
        int guardianCount,
        int quorum,
        Dictionary<string, ElectionPublicKey>? publicKeys = null,
        Dictionary<string, ElectionPartialKeyBackup>? otherBackups = null)
    {
        var storage = StorageService.GetInstance();

        var filename = GuardianPrefix + guardianId + GuardianExt;
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(basePath, PrivateKeyFolder, keyCeremonyId, filename);

        var data = storage.FromFile(filePath);
        try
        {
            var privateGuardian = JsonConvert.DeserializeObject<GuardianPrivateRecord>(
                data, SerializationSettings.NewtonsoftSettings());

            return privateGuardian != null ?
                FromPrivateRecord(privateGuardian, keyCeremonyId, guardianCount, quorum, publicKeys, otherBackups) :
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
    public static Guardian? Load(
        string guardianId,
        KeyCeremonyRecord keyCeremony,
        Dictionary<string, ElectionPublicKey>? publicKeys = null,
        Dictionary<string, ElectionPartialKeyBackup>? otherBackups = null)
    {
        return Load(guardianId, keyCeremony.KeyCeremonyId!, keyCeremony.NumberOfGuardians, keyCeremony.Quorum, publicKeys, otherBackups);
    }

    /// <summary>
    /// Saves the guardian to the local storage device
    /// </summary>
    public static void Save(this Guardian self, string keyCeremonyId)
    {
        var storage = StorageService.GetInstance();

        GuardianPrivateRecord data = self;
        var dataJson = JsonConvert.SerializeObject(data, SerializationSettings.NewtonsoftSettings());

        var filename = GuardianPrefix + data.GuardianId + GuardianExt;
        var basePath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        var filePath = Path.Combine(basePath, PrivateKeyFolder, keyCeremonyId);

        storage.UpdatePath(filePath);
        storage.ToFile(filename, dataJson);
    }
}
