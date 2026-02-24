using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class ConstantsService : BaseDatabaseService<ConstantsRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableElections;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ConstantsService() : base(_collection, nameof(ConstantsRecord)) { }

    /// <summary>
    /// Gets a constants record
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<ConstantsRecord?> GetByElectionIdAsync(string electionId)
    {
        return await GetByFieldAsync(DbConstants.ElectionId, electionId);
    }
}
