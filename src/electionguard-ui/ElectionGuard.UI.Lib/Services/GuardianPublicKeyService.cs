using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class GuardianPublicKeyService : BaseDatabaseService<GuardianPublicKey>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = Constants.TableKeyCeremonies;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public GuardianPublicKeyService() : base(_collection) { }

    /// <summary>
    /// Get all of the Public keys for a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to use</param>
    /// <returns></returns>
    public async Task<List<GuardianPublicKey>> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return await GetAllByFieldAsync(Constants.KeyCeremonyId, keyCeremonyId);
    }

    /// <summary>
    /// Update the data into the collection
    /// </summary>
    /// <param name="keyCeremony">key ceremony id to search for</param>
    /// <param name="guardianId">guardian id to search for</param>
    /// <param name="key">key to set for the guardian</param>
    virtual public async Task UpdatePublicKeyAsync(string keyCeremonyId, string guardianId, ElectionPublicKey? key)
    {
        var filterBuilder = Builders<GuardianPublicKey>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremonyId),
            filterBuilder.Eq(Constants.GuardianId, guardianId),
            filterBuilder.Eq(Constants.DataType, nameof(GuardianPublicKey)));
        
        var updateBuilder = Builders<GuardianPublicKey>.Update;
        var update = updateBuilder.Set(Constants.PublicKey, key);

        await UpdateAsync(filter, update);
    }

}
