using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;


/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class KeyCeremonyService : BaseDatabaseService<KeyCeremony>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public KeyCeremonyService() : base(_collection) { }

    /// <summary>
    /// Gets a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<KeyCeremony?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Updated the current state of the key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key cermony id to use</param>
    /// <param name="state">new state to put the key ceremony into</param>
    virtual public async Task UpdateStateAsync(string keyCeremonyId, KeyCeremonyState state)
    {
        var filterBuilder = Builders<KeyCeremony>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(Constants.DataType, nameof(KeyCeremony)));

        var updateBuilder = Builders<KeyCeremony>.Update;
        var update = updateBuilder.Set(Constants.State, state);

        await UpdateAsync(filter, update);
    }

    /// <summary>
    /// Updates the key cermeony to a compelted state and sets the completed at date/time
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to update</param>
    virtual public async Task UpdateCompleteAsync(string keyCeremonyId, ElectionJointKey jointKey)
    {
        var filterBuilder = Builders<KeyCeremony>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(Constants.DataType, nameof(KeyCeremony)));

        var updateBuilder = Builders<KeyCeremony>.Update;
        var update = updateBuilder.Set(Constants.State, KeyCeremonyState.Complete)
                                    .Set(Constants.CompletedAt, DateTime.UtcNow)
                                    .Set(Constants.JointKey, jointKey);

        await UpdateAsync(filter, update);
    }
}


/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class GuardianPublicKeyService : BaseDatabaseService<GuardianPublicKey>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public GuardianPublicKeyService() : base(_collection) { }

    /// <summary>
    /// Get all of the Public keys for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to use</param>
    /// <returns></returns>
    public async Task<List<GuardianPublicKey>> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Update the data into the collection
    /// </summary>
    /// <param name="keyCeremony">key ceremony id to search for</param>
    /// <param name="guardianId">guardian id to search for</param>
    /// <param name="key">key to set for the guardian</param>
    virtual public async Task UpdatePublicKeyAsync(string keyCeremonyId, string guardianId, ElectionPublicKey? key)
    {
        var filterBuilder = Builders<GuardianPublicKey>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(Constants.GuardianId, guardianId),
            filterBuilder.Eq(Constants.DataType, nameof(GuardianPublicKey)));
        
        var updateBuilder = Builders<GuardianPublicKey>.Update;
        var update = updateBuilder.Set(Constants.PublicKey, key);

        await UpdateAsync(filter, update);
    }

}

/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class GuardianBackupService : BaseDatabaseService<GuardianBackups>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public GuardianBackupService() : base(_collection) { }

    /// <summary>
    /// Get all of the backups for all guardians for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<List<GuardianBackups>?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Get all of the backups for a single guardian for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    /// <param name="DesignatedId">guardian id to match</param>
    /// <returns></returns>
    public async Task<List<GuardianBackups>?> GetByGuardianIdAsync(string keyCeremonyId, string DesignatedId)
    {
        var filterBuilder = Builders<GuardianBackups>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(Constants.DesignatedId, DesignatedId),
            filterBuilder.Eq(Constants.DataType, nameof(GuardianPublicKey)));

        return await GetAllByFilterAsync(filter);
    }
}

/// <summary>
/// Data Service for backup verifications
/// </summary>
public class VerificationService : BaseDatabaseService<ElectionPartialKeyVerification>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public VerificationService() : base(_collection) { }

    /// <summary>
    /// Gets a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<List<ElectionPartialKeyVerification>?> GetAllByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

}
