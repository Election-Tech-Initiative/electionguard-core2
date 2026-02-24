using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Context records
/// </summary>
public class ChallengeService : BaseDatabaseService<ChallengeRecord>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableTallies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ChallengeService() : base(_collection, nameof(ChallengeRecord)) { }

    /// <summary>
    /// Gets a context
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<List<ChallengeRecord>> GetAllByTallyIdAsync(string tallyId)
    {
        return await GetAllByFieldAsync(DbConstants.TallyId, tallyId);
    }

    public async Task<ChallengeRecord?> GetByGuardianIdAsync(string tallyId, string guardianId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(DbConstants.TallyId, tallyId),
            FilterBuilder.Eq(DbConstants.GuardianId, guardianId)
            );

        return (await GetAllByFilterAsync(filter)).FirstOrDefault();
    }
}
