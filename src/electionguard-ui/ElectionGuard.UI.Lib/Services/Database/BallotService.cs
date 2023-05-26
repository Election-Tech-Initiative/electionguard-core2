using ElectionGuard.UI.Lib.Models;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class BallotService : BaseDatabaseService<BallotRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableBallots;

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
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<IAsyncCursor<BallotRecord>> GetCursorByElectionIdAsync(string electionId)
    {
        var filter = FilterBuilder.Eq(Constants.ElectionId, electionId);
        return await GetCursorByFilterAsync(filter);
    }

    /// <summary>
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    /// <param name="state">State to filter the ballots</param>
    public async Task<IAsyncCursor<BallotRecord>> GetCursorBallotsByElectionIdStateAsync(string electionId, BallotBoxState state)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(Constants.ElectionId, electionId),
            FilterBuilder.Eq(Constants.BallotState, state));
        return await GetCursorByFilterAsync(filter);
    }



    /// <summary>
    /// Gets ballots from a single upload
    /// </summary>
    /// <param name="uploadId">upload id to search for</param>
    public async Task<List<BallotRecord>> GetByUploadIdAsync(string uploadId)
    {
        return await GetAllByFieldAsync(Constants.UploadId, uploadId);
    }

    /// <summary>
    /// Check to see if the ballot has already been included
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task<bool> BallotExistsAsync(string ballotCode)
    {
        var filterBuilder = Builders<BallotRecord>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.BallotCode, ballotCode));

        var ballotCount = await CountByFilterAsync(filter);
        return ballotCount > 0;
    }

    /// <summary>
    /// Get a ballot based on its ballot code
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task<BallotRecord?> GetByBallotCodeAsync(string ballotCode)
    {
        return await GetByFieldAsync(Constants.BallotCode, ballotCode);
    }

    /// <summary>
    /// Check to see if the ballot has already been included
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task DeleteByBallotCodeAsync(string ballotCode)
    {
        var filterBuilder = Builders<BallotRecord>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.BallotCode, ballotCode));

        await MarkAsDeletedAsync(filter);
    }
}
