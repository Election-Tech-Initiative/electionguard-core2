using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.ElectionSetup.Records;

namespace ElectionGuard.UI.Lib.Services;

public interface IGuardianBackupService : IDatabaseService<GuardianBackups>
{
    Task<long> CountAsync(string keyCeremonyId);
    Task<long> CountAsync(string keyCeremonyId, string guardianId);
    Task<List<GuardianBackups>?> GetByGuardianIdAsync(string keyCeremonyId, string DesignatedId);
    Task<List<GuardianBackups>?> GetByKeyCeremonyIdAsync(string keyCeremonyId);
}

/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class GuardianBackupService : BaseDatabaseService<GuardianBackups>, IGuardianBackupService
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public GuardianBackupService() : base(_collection, nameof(GuardianBackups)) { }

    /// <summary>
    /// Get all of the backups for all guardians for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<List<GuardianBackups>?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(DbConstants.KeyCeremonyId, keyCeremonyId);
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
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(DbConstants.DesignatedId, DesignatedId));

        return await GetAllByFilterAsync(filter);
    }

    public async Task<long> CountAsync(string keyCeremonyId)
    {
        var filterBuilder = Builders<GuardianBackups>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId));

        return await CountByFilterAsync(filter);
    }

    public async Task<long> CountAsync(string keyCeremonyId, string guardianId)
    {
        var filterBuilder = Builders<GuardianBackups>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(DbConstants.GuardianId, guardianId));

        return await CountByFilterAsync(filter);
    }
}
