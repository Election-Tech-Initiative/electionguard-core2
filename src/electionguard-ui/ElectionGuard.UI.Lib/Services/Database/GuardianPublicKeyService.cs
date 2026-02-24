using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.Guardians;

namespace ElectionGuard.UI.Lib.Services;

public interface IGuardianPublicKeyService : IDatabaseService<GuardianPublicKey>
{
    Task<long> CountAsync(string keyCeremonyId);
    Task<List<GuardianPublicKey>> GetAllByKeyCeremonyIdAsync(string keyCeremonyId);
    Task<GuardianPublicKey?> GetByIdsAsync(string keyCeremonyId, string guardianId);
    Task UpdatePublicKeyAsync(string keyCeremonyId, string guardianId, ElectionPublicKey? key);
}

/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class GuardianPublicKeyService : BaseDatabaseService<GuardianPublicKey>, IGuardianPublicKeyService
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private static readonly string _collection = DbConstants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public GuardianPublicKeyService() : base(_collection, nameof(GuardianPublicKey)) { }

    /// <summary>
    /// Get all of the Public keys for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to use</param>
    /// <returns></returns>
    public async Task<List<GuardianPublicKey>> GetAllByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(DbConstants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Update the data into the collection
    /// </summary>
    /// <param name="keyCeremony">key ceremony id to search for</param>
    /// <param name="guardianId">guardian id to search for</param>
    /// <param name="key">key to set for the guardian</param>
    public virtual async Task UpdatePublicKeyAsync(
        string keyCeremonyId, string guardianId, ElectionPublicKey? key)
    {
        var filterBuilder = Builders<GuardianPublicKey>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(DbConstants.GuardianId, guardianId));

        var update = Builders<GuardianPublicKey>.Update.Set(DbConstants.PublicKey, key!);

        await UpdateAsync(filter, update);
    }

    public async Task<long> CountAsync(string keyCeremonyId)
    {
        var filterBuilder = Builders<GuardianPublicKey>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId));

        return await CountByFilterAsync(filter);
    }

    public async Task<GuardianPublicKey?> GetByIdsAsync(string keyCeremonyId, string guardianId)
    {
        var filterBuilder = Builders<GuardianPublicKey>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(DbConstants.GuardianId, guardianId));

        var list = await GetAllByFilterAsync(filter);
        return list.FirstOrDefault();
    }

    public async Task<List<string>> GetKeyCeremonyIdsAsync(string guardianId)
    {
        var list = await GetAllByFieldAsync(DbConstants.GuardianId, guardianId);
        return list.Select(g => g.KeyCeremonyId!).ToList();
    }

}
