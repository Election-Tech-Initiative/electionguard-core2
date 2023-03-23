using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Tallies
/// </summary>
public class TallyService : BaseDatabaseService<Tally>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "tallies";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public TallyService() : base(_collection) { }

    /// <summary>
    /// Gets tallies for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<Tally>> GetByElectionIdAsync(string electionId)
    {
        return await GetAllByFieldAsync(Constants.ElectionId, electionId);
    }

}
