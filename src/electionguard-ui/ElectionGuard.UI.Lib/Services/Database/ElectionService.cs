using ElectionGuard.UI.Lib.Models;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Data Service for Elections
/// </summary>
public class ElectionService : BaseDatabaseService<Election>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = DbConstants.TableElections;

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ElectionService() : base(_collection, nameof(Election)) { }

    /// <summary>
    /// Gets an election
    /// </summary>
    /// <param name="electionId">election id to search for</param>
    public async Task<Election?> GetByElectionIdAsync(string electionId)
    {
        return await GetByFieldAsync(DbConstants.ElectionId, electionId);
    }

    /// <summary>
    /// Check if an election exists
    /// </summary>
    /// <param name="electionName">name of the election to search for</param>
    public async Task<bool> ElectionNameExists(string electionName)
    {
        var election = await GetByNameAsync(electionName);
        return election != null;
    }

    /// <summary>
    /// Updated the current date of the latest export
    /// </summary>
    /// <param name="electionId">election id to use</param>
    /// <param name="date">date to save</param>
    virtual public async Task UpdateEncryptionExportDateAsync(string electionId, DateTime date)
    {
        var filterBuilder = Builders<Election>.Filter;
        var filter = filterBuilder.And(filterBuilder.Eq(DbConstants.ElectionId, electionId));

        var updateBuilder = Builders<Election>.Update;
        var update = updateBuilder.Set(DbConstants.ExportEncryptionDateTime, date);

        await UpdateAsync(filter, update);
    }

    /// <summary>
    /// Gets count of elections that use a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<long> CountByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        var filterBuilder = Builders<Election>.Filter;
        var filter = filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId);
        return await CountByFilterAsync(filter);
    }

    /// <summary>
    /// Gets all elections that use a key ceremony
    /// </summary>
    /// <param name="keyCeremonyId">key ceremony id to search for</param>
    public async Task<List<Election>> GetAllByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        var filterBuilder = Builders<Election>.Filter;
        var filter = filterBuilder.Eq(DbConstants.KeyCeremonyId, keyCeremonyId);
        return await GetAllByFilterAsync(filter);
    }

}
