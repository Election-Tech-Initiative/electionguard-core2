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
    public KeyCeremonyService() : base(_collection, nameof(KeyCeremony)) { }

    /// <summary>
    /// Gets a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<KeyCeremony?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Gets all key ceremonies that are not in a completed state
    /// </summary>
    public async Task<List<KeyCeremony>?> GetAllNotCompleteAsync()
    {
        var filterBuilder = Builders<KeyCeremony>.Filter;
        var filter = filterBuilder.Ne(Constants.State, KeyCeremonyState.Complete);

        return await GetAllByFilterAsync(filter);
    }

    /// <summary>
    /// Gets all key ceremonies that are not in a completed state
    /// </summary>
    public async Task<List<KeyCeremony>?> GetAllCompleteAsync()
    {
        var filterBuilder = Builders<KeyCeremony>.Filter;
        var filter = filterBuilder.Eq(Constants.State, KeyCeremonyState.Complete);

        return await GetAllByFilterAsync(filter);
    }

    /// <summary>
    /// Updated the current state of the key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key cermony id to use</param>
    /// <param name="state">new state to put the key ceremony into</param>
    virtual public async Task UpdateStateAsync(string keyCeremonyId, KeyCeremonyState state)
    {
        var filterBuilder = Builders<KeyCeremony>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId));

        var updateBuilder = Builders<KeyCeremony>.Update;
        var update = updateBuilder.Set(Constants.State, state)
                                    .Set(Constants.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(filter, update);
    }

    /// <summary>
    /// Updates the key cermeony to a completed state and sets the completed at date/time
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to update</param>
    virtual public async Task UpdateCompleteAsync(string keyCeremonyId, ElectionJointKey jointKey)
    {
        var filterBuilder = Builders<KeyCeremony>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId));

        var updateBuilder = Builders<KeyCeremony>.Update;
        var update = updateBuilder.Set(Constants.State, KeyCeremonyState.Complete)
                                    .Set(Constants.CompletedAt, DateTime.UtcNow)
                                    .Set(Constants.UpdatedAt, DateTime.UtcNow)
                                    .Set(Constants.JointKey, jointKey);

        await UpdateAsync(filter, update);
    }
}
