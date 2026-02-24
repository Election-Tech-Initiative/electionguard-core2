using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

public interface IKeyCeremonyService : IDatabaseService<KeyCeremonyRecord>
{
    Task<List<KeyCeremonyRecord>?> GetAllCompleteAsync();
    Task<List<KeyCeremonyRecord>?> GetAllNotCompleteAsync();
    Task<KeyCeremonyRecord?> GetByKeyCeremonyIdAsync(string keyCeremonyId);
    Task UpdateCompleteAsync(string keyCeremonyId, ElectionJointKey jointKey);
    Task UpdateStateAsync(string keyCeremonyId, KeyCeremonyState state);
}


/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class KeyCeremonyService : BaseDatabaseService<KeyCeremonyRecord>, IKeyCeremonyService
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public KeyCeremonyService() : base(_collection, nameof(KeyCeremonyRecord)) { }

    /// <summary>
    /// Gets a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<KeyCeremonyRecord?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetByFieldAsync(DbConstants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Gets all key ceremonies that are not in a completed state
    /// </summary>
    public async Task<List<KeyCeremonyRecord>?> GetAllNotCompleteAsync()
    {
        var filterBuilder = Builders<KeyCeremonyRecord>.Filter;
        var filter = filterBuilder.Ne(DbConstants.State, KeyCeremonyState.Complete);

        return await GetAllByFilterAsync(filter);
    }

    /// <summary>
    /// Gets all key ceremonies that are not in a completed state
    /// </summary>
    public async Task<List<KeyCeremonyRecord>?> GetAllCompleteAsync()
    {
        var filterBuilder = Builders<KeyCeremonyRecord>.Filter;
        var filter = filterBuilder.Eq(DbConstants.State, KeyCeremonyState.Complete);

        return await GetAllByFilterAsync(filter);
    }

    /// <summary>
    /// Updated the current state of the key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key cermony id to use</param>
    /// <param name="state">new state to put the key ceremony into</param>
    public virtual async Task UpdateStateAsync(string keyCeremonyId, KeyCeremonyState state)
    {
        var filterBuilder = Builders<KeyCeremonyRecord>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId));

        var updateBuilder = Builders<KeyCeremonyRecord>.Update;
        var update = updateBuilder.Set(DbConstants.State, state)
                                    .Set(DbConstants.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(filter, update);
    }

    /// <summary>
    /// Updates the key cermeony to a completed state and sets the completed at date/time
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to update</param>
    public virtual async Task UpdateCompleteAsync(string keyCeremonyId, ElectionJointKey jointKey)
    {
        var filterBuilder = Builders<KeyCeremonyRecord>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId));

        var updateBuilder = Builders<KeyCeremonyRecord>.Update;
        var update = updateBuilder.Set(DbConstants.State, KeyCeremonyState.Complete)
                                    .Set(DbConstants.CompletedAt, DateTime.UtcNow)
                                    .Set(DbConstants.UpdatedAt, DateTime.UtcNow)
                                    .Set(DbConstants.JointKey, jointKey);

        await UpdateAsync(filter, update);
    }
}
