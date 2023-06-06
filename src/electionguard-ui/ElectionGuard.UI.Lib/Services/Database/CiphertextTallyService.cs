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
    public CiphertextTallyService() : base(_collection, nameof(CiphertextTallyRecord)) { }

    /// <summary>
    /// Gets ciphertext tally for an election
    /// </summary>
    /// <param name="tallyId">tally id to search for</param>
    public async Task<CiphertextTallyRecord?> GetByTallyIdAsync(string tallyId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(Constants.TallyId, tallyId),
            FilterBuilder.Eq(Constants.IsExportable, true));
        return (await GetAllByFilterAsync(filter)).LastOrDefault();
    }

    /// <summary>
    /// Gets a list of all ciphertext tally records for an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<CiphertextTallyRecord>> GetAllByElectionIdAsync(string electionId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(Constants.ElectionId, electionId),
            FilterBuilder.Eq(Constants.IsExportable, false),
            FilterBuilder.Eq(Constants.TallyId, string.Empty));
        return await GetAllByFilterAsync(filter);
    }

    /// <summary>
    /// Gets ciphertext tally for a ballot upload
    /// </summary>
    /// <param name="uploadId">upload id to search for</param>
    public async Task<CiphertextTallyRecord?> GetByUploadIdIdAsync(string uploadId)
    {
        return await GetByFieldAsync(Constants.UploadId, uploadId);
    }

}
