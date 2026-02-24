using ElectionGuard.UI.Lib.Models;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class BallotUploadService : BaseDatabaseService<BallotUpload>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableBallots;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public BallotUploadService() : base(_collection, nameof(BallotUpload)) { }

    /// <summary>
    /// Gets an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<BallotUpload>> GetByElectionIdAsync(string electionId)
    {
        return await GetAllByFieldAsync(DbConstants.ElectionId, electionId);
    }

    public async Task<bool> DriveUsed(long serialNumber, string electionId)
    {
        var filterBuilder = Builders<BallotUpload>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.SerialNumber, serialNumber), filterBuilder.Eq(DbConstants.ElectionId, electionId));

        var uploadCount = await CountByFilterAsync(filter);
        return uploadCount > 0;
    }

    public async Task DecrementBallotsChallenged(string uploadId)
    {
        var filter = FilterBuilder.And(FilterBuilder.Eq(DbConstants.UploadId, uploadId));
        var upload = await GetByFieldAsync(DbConstants.UploadId, uploadId);
        var update = Builders<BallotUpload>.Update
                        .Set(DbConstants.BallotChallenged, upload!.BallotChallenged - 1)
                        .Set(DbConstants.BallotSpoiled, upload!.BallotSpoiled + 1);

        await UpdateAsync(filter, update);
    }
}
