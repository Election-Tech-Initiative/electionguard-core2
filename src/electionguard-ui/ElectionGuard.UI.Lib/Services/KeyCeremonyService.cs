using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;
using System.Security.Cryptography.X509Certificates;

namespace ElectionGuard.UI.Lib.Services;


/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class KeyCeremonyService : BaseDatabaseService<KeyCeremony>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "key_ceremonies";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public KeyCeremonyService() : base(_collection) { }

    public async Task<KeyCeremony?> GetByKeyCeremonyId(string id)
    {
        return await GetByFieldAsync(Constants.KeyCeremonyId, id);
    }

    virtual public async Task UpdateStateAsync(string keyCeremony, KeyCeremonyState state)
    {
        var filterBuilder = Builders<KeyCeremony>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremony),
            filterBuilder.Eq(Constants.DataType, nameof(KeyCeremony)));

        var updateBuilder = Builders<KeyCeremony>.Update;
        var update = updateBuilder.Set(Constants.State, state);

        await UpdateAsync(UpdateFilter(filter), update);
    }
}


/// <summary>
/// Data Service for Key Ceremonies
/// </summary>
public class GuardianPublicKeyService : BaseDatabaseService<GuardianPublicKey>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "key_ceremonies";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public GuardianPublicKeyService() : base(_collection) { }

    public async Task<List<GuardianPublicKey>> GetByKeyCeremonyId(string id)
    {
        return await GetAllByFieldAsync(Constants.KeyCeremonyId, id);
    }

    /// <summary>
    /// Update the data into the collection
    /// </summary>
    /// <param name="keyCeremony">key ceremony id to search for</param>
    /// <param name="guardianId">guardian id to search for</param>
    /// <param name="key">key to set for the guardian</param>
    virtual public async Task UpdatePublicKeyAsync(string keyCeremony, string guardianId, ElectionPublicKey? key)
    {
        var filterBuilder = Builders<GuardianPublicKey>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(Constants.KeyCeremonyId, keyCeremony),
            filterBuilder.Eq(Constants.GuardianId, guardianId),
            filterBuilder.Eq(Constants.DataType, nameof(GuardianPublicKey)));
        
        var updateBuilder = Builders<GuardianPublicKey>.Update;
        var update = updateBuilder.Set(Constants.PublicKey, key);

        await UpdateAsync(UpdateFilter(filter), update);
    }

}

