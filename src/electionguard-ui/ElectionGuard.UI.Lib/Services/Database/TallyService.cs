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
    /// Gets tallies for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<TallyRecord>> GetByElectionIdAsync(string electionId)
    {
        return await GetAllByFieldAsync(Constants.ElectionId, electionId);
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
        var filterBuilder = Builders<TallyRecord>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.TallyId, tallyId));

        var updateBuilder = Builders<TallyRecord>.Update;
        var update = updateBuilder.Set(Constants.State, TallyState.Complete)
                                    .Set(Constants.CompletedAt, DateTime.UtcNow)
                                    .Set(Constants.UpdatedAt, DateTime.UtcNow);

        await UpdateAsync(filter, update);
    }
}
