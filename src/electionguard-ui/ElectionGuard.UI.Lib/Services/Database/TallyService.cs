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
    private readonly static string _collection = DbConstants.TableTallies;

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
            FilterBuilder.Eq(DbConstants.ElectionId, electionId),
            FilterBuilder.Ne(DbConstants.State, TallyState.Abandoned));

        return await GetAllByFilterAsync(filter);
    }

    /// <summary>
    /// Gets tally state to determine if it is running
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<bool> IsRunningByTallyIdAsync(string tallyId)
    {
        var tally = await GetByFieldAsync(DbConstants.TallyId, tallyId);
        return tally?.State is not TallyState.Complete and not TallyState.Abandoned;
    }

    /// <summary>
    /// Gets tallies for an election
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<TallyRecord?> GetByTallyIdAsync(string tallyId)
    {
        return await GetByFieldAsync(DbConstants.TallyId, tallyId);
    }

    public async Task<bool> TallyNameExistsAsync(string name)
    {
        var tally = await GetByNameAsync(name);
        return tally != null;
    }

    public async Task<List<TallyRecord>> GetAllByKeyCeremoniesAsync(List<string> ids)
    {
        BsonArray array = new(ids);
        return await GetAllByFieldInListAsync(DbConstants.KeyCeremonyId, array);
    }

    /// <summary>
    /// Updates the key cermeony to a completed state and sets the completed at date/time
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to update</param>
    virtual public async Task UpdateCompleteAsync(string tallyId)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(DbConstants.TallyId, tallyId));

        var updateBuilder = Builders<TallyRecord>.Update;
        var update = updateBuilder.Set(DbConstants.State, TallyState.Complete)
                                    .Set(DbConstants.CompletedAt, DateTime.UtcNow)
                                    .Set(DbConstants.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(filter, update);
    }

    /// <summary>
    /// Update the state of the TallyRecord
    /// </summary>
    /// <param name="tallyId">tally id to use</param>
    /// <param name="state">new state to put the tally into</param>
    public virtual async Task UpdateStateAsync(string tallyId, TallyState state)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(DbConstants.TallyId, tallyId));

        var updateBuilder = Builders<TallyRecord>.Update;
        var update = updateBuilder.Set(DbConstants.State, state);

        await UpdateAsync(filter, update);
    }

}
