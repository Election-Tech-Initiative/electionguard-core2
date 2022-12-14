using MongoDB.Driver;
using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Interface for defining the basic calls for the database for a given data type.
/// Any other functions that are not generic across all of the types can be added to
/// the specific Service created by this interface.
/// </summary>
/// <typeparam name="T">Data type to use for the service</typeparam>
internal interface IDatabaseService<T>
{
    Task<List<T>> GetAllAsync(string? table = null);
    Task<List<T>> GetAllByFieldAsync(string fieldName, object fieldValue, string? table = null);
    Task<T?> GetByIdAsync(string id, string? table = null);
    Task<T?> GetByNameAsync(string name, string? table = null);
    Task<T?> GetByFieldAsync(string fieldName, object fieldValue, string? table = null);
    Task SaveAsync(T data, string? table = null);
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
    public async Task SaveAsync(T data, string? table = null)
    {
        var collection = DbService.GetCollection<T>(table ?? _collection);
        await collection.InsertOneAsync(data);
    }

    /// <summary>
    /// Get all of a data type from a given collection
    /// </summary>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection</returns>
    public async Task<List<T>> GetAllAsync(string? table = null)
    {
        var data = DbService.GetCollection<T>(table ?? _collection);
        var item = await data.FindAsync<T>(Builders<T>.Filter.Empty);
        return item.ToList();
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
        var data = DbService.GetCollection<T>(table ?? _collection);
        var filter = Builders<T>.Filter.Eq(fieldName, fieldValue);
        var item = await data.FindAsync<T>(filter);
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
        return await GetByFieldAsync("Id", id, table);
    }

    /// <summary>
    /// Get the document from the database with the provided name
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>Document that matches the given name</returns>
    public async Task<T?> GetByNameAsync(string name, string? table = null)
    {
        return await GetByFieldAsync("Name", name, table);
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
}

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
