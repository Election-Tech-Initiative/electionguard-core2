using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Tallies
/// </summary>
public class PlaintextTallyService : BaseDatabaseService<PlaintextTallyRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public PlaintextTallyService() : base(_collection, nameof(PlaintextTallyRecord)) { }

    /// <summary>
    /// Gets plain text tally for an election
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<PlaintextTallyRecord?> GetByTallyIdAsync(string tallyId)
    {
        return await GetByFieldAsync(DbConstants.TallyId, tallyId);
    }

    public Task<PlaintextTallyRecord> SaveAsync(PlaintextTallyRecord data)
    {
        var filter = FilterBuilder.Eq(DbConstants.TallyId, data.TallyId);
        return SaveAsync(data, filter);
    }


}
