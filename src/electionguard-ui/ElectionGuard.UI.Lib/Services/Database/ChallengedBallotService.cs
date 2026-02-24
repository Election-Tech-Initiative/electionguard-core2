using ElectionGuard.UI.Lib.Models;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class ChallengedBallotService : BaseDatabaseService<ChallengedBallotRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableBallots;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ChallengedBallotService() : base(_collection, nameof(ChallengedBallotRecord)) { }

    public Task<ChallengedBallotRecord> SaveAsync(ChallengedBallotRecord data)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(DbConstants.BallotCode, data.BallotCode), FilterBuilder.Eq(DbConstants.TallyId, data.TallyId));
        return SaveAsync(data, filter);
    }

    /// <summary>
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<ChallengedBallotRecord>> GetByElectionIdAsync(string electionId)
    {
        return await GetAllByFieldAsync(DbConstants.ElectionId, electionId);
    }

    /// <summary>
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<IAsyncCursor<ChallengedBallotRecord>> GetCursorByElectionIdAsync(string electionId)
    {
        var filter = FilterBuilder.Eq(DbConstants.ElectionId, electionId);
        return await GetCursorByFilterAsync(filter);
    }

    /// <summary>
    /// Gets ballots from a single upload
    /// </summary>
    /// <param name="uploadId">upload id to search for</param>
    public async Task<List<ChallengedBallotRecord>> GetByUploadIdAsync(string uploadId)
    {
        return await GetAllByFieldAsync(DbConstants.UploadId, uploadId);
    }

    /// <summary>
    /// Check to see if the ballot has already been included
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task<bool> BallotExistsAsync(string ballotCode)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(DbConstants.BallotCode, ballotCode));

        var ballotCount = await CountByFilterAsync(filter);
        return ballotCount > 0;
    }

    /// <summary>
    /// Get a ballot based on its ballot code
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task<ChallengedBallotRecord?> GetByBallotCodeAsync(string ballotCode)
    {
        return await GetByFieldAsync(DbConstants.BallotCode, ballotCode);
    }

    /// <summary>
    /// Check to see if the ballot has already been included
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task DeleteByBallotCodeAsync(string ballotCode)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(DbConstants.BallotCode, ballotCode));

        await MarkAsDeletedAsync(filter);
    }
}
