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
    private readonly static string _collection = DbConstants.TableBallots;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public BallotService() : base(_collection, nameof(BallotRecord)) { }

    /// <summary>
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<BallotRecord?> GetByOjectIdAsync(string electionId)
    {
        return await GetByFieldAsync(DbConstants.ObjectId, electionId);
    }

    /// <summary>
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<BallotRecord>> GetByElectionIdAsync(string electionId)
    {
        return await GetAllByFieldAsync(DbConstants.ElectionId, electionId);
    }

    /// <summary>
    /// Gets ballots for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<IAsyncCursor<BallotRecord>> GetCursorByElectionIdAsync(string electionId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(DbConstants.ElectionId, electionId),
            FilterBuilder.Ne(DbConstants.BallotState, BallotBoxState.Spoiled));
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
            FilterBuilder.Eq(DbConstants.ElectionId, electionId),
            FilterBuilder.Eq(DbConstants.BallotState, state));
        return await GetCursorByFilterAsync(filter);
    }

    public async Task<long> GetCountBallotsByElectionIdStateAsync(string electionId, BallotBoxState state)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(DbConstants.ElectionId, electionId),
            FilterBuilder.Eq(DbConstants.BallotState, state));
        return await CountByFilterAsync(filter);
    }

    /// <summary>
    /// Gets ballots from a single upload
    /// </summary>
    /// <param name="uploadId">upload id to search for</param>
    public async Task<List<BallotRecord>> GetByUploadIdAsync(string uploadId)
    {
        return await GetAllByFieldAsync(DbConstants.UploadId, uploadId);
    }

    /// <summary>
    /// Check to see if the ballot has already been included
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task<bool> BallotExistsAsync(string ballotCode)
    {
        var filter = FilterBuilder.Eq(DbConstants.BallotCode, ballotCode);

        var ballotCount = await CountByFilterAsync(filter);
        return ballotCount > 0;
    }

    /// <summary>
    /// Get a ballot based on its ballot code
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task<BallotRecord?> GetByBallotCodeAsync(string ballotCode)
    {
        return await GetByFieldAsync(DbConstants.BallotCode, ballotCode);
    }

    /// <summary>
    /// Check to see if the ballot has already been included
    /// </summary>
    /// <param name="ballotCode">ballotcode to find</param>
    public async Task DeleteByBallotCodeAsync(string ballotCode)
    {
        var filter = FilterBuilder.Eq(DbConstants.BallotCode, ballotCode);

        await MarkAsDeletedAsync(filter);
    }

    /// <summary>
    /// Move a ballot into a spoiled state
    /// </summary>
    /// <param name="ballotCode">ballot code for the ballot to convert</param>
    public async Task ConvertToSpoiledByBallotCodeAsync(string ballotCode)
    {
        var filter = FilterBuilder.And(
                        FilterBuilder.Eq(DbConstants.BallotCode, ballotCode),
                        FilterBuilder.Ne(DbConstants.BallotState, BallotBoxState.Cast));
        var update = Builders<BallotRecord>.Update.Set(DbConstants.BallotState, BallotBoxState.Spoiled);

        await UpdateAsync(filter, update);
    }




}
