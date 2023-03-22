using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class ElectionService : BaseDatabaseService<Election>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "elections";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ElectionService() : base(_collection, nameof(Election)) { }

    /// <summary>
    /// Gets an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<Election?> GetByElectionIdAsync(string electionId)
    {
        return await GetByFieldAsync(Constants.ElectionId, electionId);
    }

    public async Task<bool> ElectionNameExists(string electionName)
    {
        var election = await GetByNameAsync(electionName);
        return election != null;
    }

}
