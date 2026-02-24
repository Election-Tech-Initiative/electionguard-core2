using ElectionGuard.UI.Lib.Models;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

public interface IVerificationService : IDatabaseService<ElectionPartialKeyVerification>
{
    Task<long> CountAsync(string keyCeremonyId, string guardianId);
    Task<long> CountAsync(string keyCeremonyId);
    Task<List<ElectionPartialKeyVerification>?> GetAllByKeyCeremonyIdAsync(string keyCeremonyId);
}

/// <summary>
/// Data Service for backup verifications
/// </summary>
public class VerificationService : BaseDatabaseService<ElectionPartialKeyVerification>, IVerificationService
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public VerificationService() : base(_collection, nameof(ElectionPartialKeyVerification)) { }

    /// <summary>
    /// Gets a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<List<ElectionPartialKeyVerification>?> GetAllByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(DbConstants.KeyCeremonyId, keyCeremonyId);
    }

    public async Task<long> CountAsync(string keyCeremonyId, string guardianId)
    {
        var filterBuilder = Builders<ElectionPartialKeyVerification>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(DbConstants.DesignatedId, guardianId));

        return await CountByFilterAsync(filter);
    }
    public async Task<long> CountAsync(string keyCeremonyId)
    {
        var filterBuilder = Builders<ElectionPartialKeyVerification>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId));

        return await CountByFilterAsync(filter);
    }
}
