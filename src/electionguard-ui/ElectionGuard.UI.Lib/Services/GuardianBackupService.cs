using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class GuardianBackupService : BaseDatabaseService<GuardianBackups>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public GuardianBackupService() : base(_collection) { }

    /// <summary>
    /// Get all of the backups for all guardians for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<List<GuardianBackups>?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Get all of the backups for a single guardian for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    /// <param name="DesignatedId">guardian id to match</param>
    /// <returns></returns>
    public async Task<List<GuardianBackups>?> GetByGuardianIdAsync(string keyCeremonyId, string DesignatedId)
    {
        var filterBuilder = Builders<GuardianBackups>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(Constants.DesignatedId, DesignatedId),
            filterBuilder.Eq(Constants.DataType, nameof(GuardianPublicKey)));

        return await GetAllByFilterAsync(filter);
    }
}
