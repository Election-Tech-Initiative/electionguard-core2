using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// static class used for constant strings used in database calls
/// </summary>
internal static class Constants
{
    public readonly static string DataType = "DataType";

    public readonly static string DesignatedId = "DesignatedId";

    public readonly static string GuardianId = "GuardianId";

    public readonly static string Id = "Id";

    public readonly static string KeyCeremonyId = "KeyCeremonyId";

    public readonly static string Name = "Name";

    public readonly static string PublicKey = "PublicKey";

    public readonly static string SoftDeleted = "SoftDeleted";

    public readonly static string State = "State";

    public readonly static string TableKeyCeremonies = "key_ceremonies";

    public readonly static string CompletedAt = "CompletedAt";

    public readonly static string JointKey = "JointKey";

    
}


/// <summary>
/// Interface for defining the basic calls for the database for a given data type.
/// Any other functions that are not generic across all of the types can be added to
/// the specific Service created by this interface.
/// </summary>
/// <typeparam name="T">Data type to use for the service</typeparam>
internal interface IDatabaseService<T>
{
    Task<List<T>> GetAllAsync(string? table = null);
    Task<List<T>> GetAllByFilterAsync(FilterDefinition<T> filter, string? table = null);
    Task<List<T>> GetAllByFieldAsync(string fieldName, object fieldValue, string? table = null);
    Task<T?> GetByIdAsync(string id, string? table = null);
    Task<T?> GetByNameAsync(string name, string? table = null);
    Task<T?> GetByFieldAsync(string fieldName, object fieldValue, string? table = null);
    Task<T> SaveAsync(T data, string? table = null);
    Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, string? table = null);
    FilterDefinition<T> UpdateFilter(FilterDefinition<T> filter, bool getDeleted = false);
}

/// <summary>
/// Base class used to make services for each data type being saved into the db.
/// This base class will have all of the common calls that will be used on all 
/// data types and collections. When a specific call to get data that is unique
/// to a data type, those calls will go into the specific class that was created
/// with this base class.
/// </summary>
/// <typeparam name="T">The datatype being saved</typeparam>
public class BaseDatabaseService<T> : IDatabaseService<T>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly string? _collection = null;

    /// <summary>
    /// Constructor to set the collection name that will be used for this data type
    /// </summary>
    /// <param name="collection">Name of the collection to use for this data type</param>
    public BaseDatabaseService(string collection)
    {
        _collection = collection;
    }

    /// <summary>
    /// Save the data into the collection
    /// </summary>
    /// <param name="data">data to be saved</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns></returns>
    virtual public async Task<T> SaveAsync(T data, string? table = null)
    {
        var collection = DbService.GetCollection<T>(table ?? _collection);
        await collection.InsertOneAsync(data);
        return data;
    }

    /// <summary>
    /// Update the data into the collection
    /// </summary>
    /// <param name="data">data to be updated</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns></returns>
    virtual public async Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, string? table = null)
    {
        var collection = DbService.GetCollection<T>(table ?? _collection);
        await collection.UpdateOneAsync(UpdateFilter(filter), update);
    }

    /// <summary>
    /// Get all of a data type from a given collection
    /// </summary>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection</returns>
    public async Task<List<T>> GetAllAsync(string? table = null)
    {
        var builder = Builders<T>.Filter;
        var filter = builder.Eq(Constants.DataType, nameof(T));
        return await GetAllByFilterAsync(filter);
    }

    /// <summary>
    /// Get the document from the database with the provided value for a field
    /// </summary>
    /// <param name="fieldName">Field name to search for</param>
    /// <param name="fieldValue">Value to search for</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection that matches the given field</returns>
    public async Task<List<T>> GetAllByFieldAsync(string fieldName, object fieldValue, string? table = null)
    {
        var builder = Builders<T>.Filter;
        var filter = builder.And(builder.Eq(fieldName, fieldValue), builder.Eq(Constants.DataType, nameof(T)));
        return await GetAllByFilterAsync(filter, table);
    }

    /// <summary>
    /// Get all of a data type from a given collection using a filter
    /// </summary>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection</returns>
    public async Task<List<T>> GetAllByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        var data = DbService.GetCollection<T>(table ?? _collection);
        var item = await data.FindAsync<T>(UpdateFilter(filter));
        return item.ToList();
    }

    /// <summary>
    /// Get the document from the database with the provided id
    /// </summary>
    /// <param name="id">Id to search for</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>Document that matches the given id</returns>
    public async Task<T?> GetByIdAsync(string id, string? table= null)
    {
        return await GetByFieldAsync(Constants.Id, id, table);
    }

    /// <summary>
    /// Get the document from the database with the provided name
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>Document that matches the given name</returns>
    virtual public async Task<T?> GetByNameAsync(string name, string? table = null)
    {
        return await GetByFieldAsync(Constants.Name, name, table);
    }

    /// <summary>
    /// Get the document from the database with the provided value for a field
    /// </summary>
    /// <param name="fieldName">Field name to search for</param>
    /// <param name="fieldValue">Value to search for</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>Document that matches the given field</returns>
    public async Task<T?> GetByFieldAsync(string fieldName, object fieldValue, string? table = null)
    {
        var list = await GetAllByFieldAsync(fieldName, fieldValue, table);
        return list.FirstOrDefault();
    }

    /// <summary>
    /// Updates the filter to only get non-deleted items
    /// </summary>
    /// <param name="filter">filter to base from</param>
    /// <returns>New filter adding soft delete as false</returns>
    public FilterDefinition<T> UpdateFilter(FilterDefinition<T> filter, bool getDeleted = false)
    {
        var builder = Builders<T>.Filter;
        return builder.And(filter, builder.Eq(Constants.SoftDeleted, getDeleted));
    }
}

/// <summary>
/// Data Service for Users
/// </summary>
public class UserService : BaseDatabaseService<User>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "users";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public UserService() : base(_collection) { }
}

/// <summary>
/// Data Service for Elections
/// </summary>
public class ElectionService : BaseDatabaseService<Election>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "elections";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public ElectionService() : base(_collection) { }
}

/// <summary>
/// Data Service for Tallies
/// </summary>
public class TallyService : BaseDatabaseService<Tally>
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly static string _collection = "tallies";

    /// <summary>
    /// Default constructor that sets the collection name
    /// </summary>
    public TallyService() : base(_collection) { }
}
