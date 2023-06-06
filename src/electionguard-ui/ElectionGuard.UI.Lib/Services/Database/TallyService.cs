using ElectionGuard.UI.Lib.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Tallies
/// </summary>
public class TallyService : BaseDatabaseService<TallyRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public TallyService() : base(_collection, nameof(TallyRecord)) { }

    /// <summary>
    /// Gets tallies for an election, including completed tallies.
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<TallyRecord>> GetAllActiveByElectionIdAsync(string electionId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(Constants.ElectionId, electionId),
            FilterBuilder.Ne(Constants.State, TallyState.Abandoned));

        return await GetAllByFilterAsync(filter);

    }

    /// <summary>
    /// Gets tallies for an election
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<TallyRecord?> GetByTallyIdAsync(string tallyId)
    {
        return await GetByFieldAsync(Constants.TallyId, tallyId);
    }

    public async Task<bool> TallyNameExistsAsync(string name)
    {
        var tally = await GetByNameAsync(name);
        return tally != null;
    }

    public async Task<List<TallyRecord>> GetAllByKeyCeremoniesAsync(List<string> ids)
    {
        BsonArray array = new(ids);
        return await GetAllByFieldInListAsync(Constants.KeyCeremonyId, array);
    }

    /// <summary>
    /// Updates the key cermeony to a completed state and sets the completed at date/time
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to update</param>
    virtual public async Task UpdateCompleteAsync(string tallyId)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(Constants.TallyId, tallyId));

        var updateBuilder = Builders<TallyRecord>.Update;
        var update = updateBuilder.Set(Constants.State, TallyState.Complete)
                                    .Set(Constants.CompletedAt, DateTime.UtcNow)
                                    .Set(Constants.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(filter, update);
    }

    /// <summary>
    /// Update the state of the TallyRecord
    /// </summary>
    /// <param name="tallyId">tally id to use</param>
    /// <param name="state">new state to put the tally into</param>
    public virtual async Task UpdateStateAsync(string tallyId, TallyState state)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(Constants.TallyId, tallyId));

        var updateBuilder = Builders<TallyRecord>.Update;
        var update = updateBuilder.Set(Constants.State, state);

        await UpdateAsync(filter, update);
    }

}
