using ElectionGuard.UI.Lib.Models;
using MongoDB.Bson;
using MongoDB.Driver;

namespace ElectionGuard.UI.Lib.Services;

/// <summary>
/// Base class used to make services for each data type being saved into the db.
/// This base class will have all of the common calls that will be used on all 
/// data types and collections. When a specific call to get data that is unique
/// to a data type, those calls will go into the specific class that was created
/// with this base class.
/// </summary>
/// <typeparam name="T">The datatype being saved</typeparam>
public class BaseDatabaseService<T> : IDatabaseService<T> where T : DatabaseRecord
{
    /// <summary>
    /// The collection name to use to get/save data into
    /// </summary>
    private readonly string? _collection = null;

    private readonly string? _type = null;

    /// <summary>
    /// Constructor to set the collection name that will be used for this data type
    /// </summary>
    /// <param name="collection">Name of the collection to use for this data type</param>
    public BaseDatabaseService(string collection, string? type = null)
    {
        _collection = collection;
        _type = type;
    }

    /// <summary>
    /// Save the data into the collection
    /// </summary>
    /// <param name="data">data to be saved</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns></returns>
    public virtual async Task<IEnumerable<T>> SaveManyAsync(IEnumerable<T> data, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);
            await collection.InsertManyAsync(data);
            return data;
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "SaveManyAsync call failed" });
            throw new ElectionGuardException("SaveManyAsync call failed", ex);
        }
    }

    /// <summary>
    /// Save the data into the collection
    /// </summary>
    /// <param name="data">data to be saved</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns></returns>
    public virtual async Task<T> SaveAsync(T data, FilterDefinition<T>? customFilter = null, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);
            var filter = FilterBuilder.Eq(DbConstants.Id, data.Id);
            _ = await collection.ReplaceOneAsync(UpdateFilter(customFilter ?? filter), data, new ReplaceOptions { IsUpsert = true });
            return data;
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "SaveAsync call failed" });
            throw new ElectionGuardException("SaveAsync call failed", ex);
        }
    }

    /// <summary>
    /// Update the data into the collection
    /// </summary>
    /// <param name="data">data to be updated</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns></returns>
    public virtual async Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);
            _ = await collection.UpdateOneAsync(UpdateFilter(filter), update);
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "Error updating a record" });
            throw new ElectionGuardException("Error updating a record", ex);
        }
    }

    /// <summary>
    /// Get all of a data type from a given collection
    /// </summary>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection</returns>
    public async Task<List<T>> GetAllAsync(string? table = null)
    {
        return await GetAllByFilterAsync(Builders<T>.Filter.Empty);
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
        var filter = FilterBuilder.Eq(fieldName, fieldValue);
        return await GetAllByFilterAsync(filter, table);
    }

    /// <summary>
    /// Get the document from the database with the provided value for a field
    /// </summary>
    /// <param name="fieldName">Field name to search for</param>
    /// <param name="fieldValues">List of values to match for search</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection that matches the given field</returns>
    public async Task<List<T>> GetAllByFieldInListAsync(string fieldName, BsonArray fieldValues, string? table = null)
    {
        var filter = FilterBuilder.In(fieldName, fieldValues);
        return await GetAllByFilterAsync(filter, table);
    }



    public FilterDefinitionBuilder<T> FilterBuilder => Builders<T>.Filter;

    /// <summary>
    /// Get all of a data type from a given collection using a filter
    /// </summary>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection</returns>
    public async Task<List<T>> GetAllByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);
            var item = await collection.FindAsync<T>(UpdateFilter(filter));
            return item?.ToList() ?? new();
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "Error updating a record" });
            
            throw new ElectionGuardException("Error getting all by filter", ex);
            //return new();
        }
    }

    /// <summary>
    /// Get all of a data type from a given collection using a filter
    /// </summary>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>List of the documents found in the given collection</returns>
    public async Task<IAsyncCursor<T>> GetCursorByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);
            return await collection.FindAsync<T>(UpdateFilter(filter));
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "Error getting all by filter" });
            throw new ElectionGuardException("Error getting all by filter", ex);
        }
    }


    /// <summary>
    /// Get the document from the database with the provided id
    /// </summary>
    /// <param name="id">Id to search for</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>Document that matches the given id</returns>
    public async Task<T?> GetByIdAsync(string id, string? table = null)
    {
        return await GetByFieldAsync(DbConstants.Id, id, table);
    }

    /// <summary>
    /// Get the document from the database with the provided name
    /// </summary>
    /// <param name="name">Name to search for</param>
    /// <param name="table">Optional parameter to allow data type to use a different collection</param>
    /// <returns>Document that matches the given name</returns>
    public virtual async Task<T?> GetByNameAsync(string name, string? table = null)
    {
        return await GetByFieldAsync(DbConstants.Name, name, table);
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
        return builder.And(filter,
                           builder.Eq(DbConstants.SoftDeleted, getDeleted),
                           builder.Eq(DbConstants.DataType, _type));
    }

    /// <summary>
    /// Get count of the number of documents that match the filter
    /// </summary>
    /// <param name="filter">filter used to search</param>
    /// <param name="table">collection to use</param>
    public async Task<long> CountByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);
            return await collection.CountDocumentsAsync(UpdateFilter(filter));
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "Error CountByFilterAsync" });
            throw new ElectionGuardException("Error CountByFilterAsync", ex);
        }
    }

    /// <summary>
    /// Get existance of documents that match the filter
    /// </summary>
    /// <param name="filter">filter used to search</param>
    /// <param name="table">collection to use</param>
    public async Task<bool> ExistsByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);
            return await collection.CountDocumentsAsync(UpdateFilter(filter)) != 0;
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "Error ExistsByFilterAsync" });
            throw new ElectionGuardException("Error ExistsByFilterAsync", ex);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="filter">filter to use to find the item to delete</param>
    /// <param name="deleted">Set the SoftDeleted field value</param>
    /// <param name="table">collection to use</param>
    public async Task MarkAsDeletedAsync(FilterDefinition<T> filter, bool deleted = true, string? table = null)
    {
        try
        {
            var collection = DbService.GetCollection<T>(table ?? _collection);

            var updateBuilder = Builders<T>.Update;
            var update = updateBuilder.Set(DbConstants.SoftDeleted, deleted);

            _ = await collection.UpdateOneAsync(UpdateFilter(filter), update);
        }
        catch (Exception ex)
        {
            DbService.OnDatabaseDisconnect(new DbEventArgs { Message = "Error MarkAsDeletedAsync" });
            throw new ElectionGuardException("Error MarkAsDeletedAsync", ex);
        }
    }

}
