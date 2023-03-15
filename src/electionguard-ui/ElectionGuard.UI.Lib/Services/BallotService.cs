using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class BallotService : BaseDatabaseService<BallotRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "ballots";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public BallotService() : base(_collection, nameof(BallotRecord)) { }

    /// <summary>
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<BallotRecord>> GetByElectionIdAsync(string electionId)
    {
        return await GetAllByFieldAsync(Constants.ElectionId, electionId);
    }

    /// <summary>
    /// Gets ballots from a single upload
    /// </summary>
    /// <param name="uploadId">upload id to search for</param>
    public async Task<List<BallotRecord>> GetByUploadIdAsync(string uploadId)
    {
        return await GetAllByFieldAsync(Constants.UploadId, uploadId);
    }
}
