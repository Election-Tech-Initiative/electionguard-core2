using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Context records
/// </summary>
public class ContextService : BaseDatabaseService<ContextRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "elections";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ContextService() : base(_collection, nameof(ContextRecord)) { }

    /// <summary>
    /// Gets a context
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<ContextRecord?> GetByElectionIdAsync(string electionId)
    {
        return await GetByFieldAsync(Constants.ElectionId, electionId);
    }

}
