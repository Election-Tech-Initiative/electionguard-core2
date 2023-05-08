using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for CiphertextTallyRecord
/// </summary>
public class CiphertextTallyService : BaseDatabaseService<CiphertextTallyRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public CiphertextTallyService() : base(_collection) { }

    /// <summary>
    /// Gets ciphertext tally for an election
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<CiphertextTallyRecord?> GetByTallyIdAsync(string tallyId)
    {
        return await GetByFieldAsync(Constants.TallyId, tallyId);
    }
}
