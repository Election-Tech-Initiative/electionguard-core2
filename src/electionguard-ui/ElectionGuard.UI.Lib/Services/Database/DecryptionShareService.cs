using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Context records
/// </summary>
public class DecryptionShareService : BaseDatabaseService<DecryptionShareRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public DecryptionShareService() : base(_collection, nameof(DecryptionShareRecord)) { }

    /// <summary>
    /// Gets a context
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<DecryptionShareRecord>> GetAllByTallyIdAsync(string tallyId)
    {
        return await GetAllByFieldAsync(DbConstants.TallyId, tallyId);
    }

    public async Task<DecryptionShareRecord?> GetByGuardianIdAsync(string tallyId, string guardianId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(DbConstants.TallyId, tallyId),
            FilterBuilder.Eq(DbConstants.GuardianId, guardianId)
            );

        return (await GetAllByFilterAsync(filter)).FirstOrDefault();
    }

    public async Task<long> GetCountByTallyAsync(string tallyId)
    {
        var filter = FilterBuilder.Eq(DbConstants.TallyId, tallyId);
        return await CountByFilterAsync(filter);
    }

    public async Task<bool> GetExistsByTallyAsync(string tallyId, string guardianId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(DbConstants.TallyId, tallyId),
            FilterBuilder.Eq(DbConstants.GuardianId, guardianId)
            );
        return await ExistsByFilterAsync(filter);
    }

}
