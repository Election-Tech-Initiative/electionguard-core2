using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Tallies
/// </summary>
public class MultiTallyService : BaseDatabaseService<MultiTallyRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public MultiTallyService() : base(_collection, nameof(MultiTallyRecord)) { }

    /// <summary>
    /// Gets multi tallies for an election
    /// </summary>
    /// <param name="multiTallyId">multi tally id to search for</param>
    public async Task<MultiTallyRecord?> GetByMultiTallyIdAsync(string multiTallyId)
    {
        return await GetByFieldAsync(DbConstants.MultiTallyId, multiTallyId);
    }



}
