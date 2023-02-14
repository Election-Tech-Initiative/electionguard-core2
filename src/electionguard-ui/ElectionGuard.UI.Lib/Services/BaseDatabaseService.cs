﻿using ElectionGuard.UI.Lib.Models;
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

    /// <summary>
    /// Get count of the number of documents that match the filter
    /// </summary>
    /// <param name="filter">filter used to search</param>
    /// <param name="table">collection to use</param>
    public async Task<long> CountByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        var data = DbService.GetCollection<T>(table ?? _collection);
        return await data.CountDocumentsAsync(filter);
    }
}