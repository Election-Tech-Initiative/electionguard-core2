using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectionGuard.UI.Lib.Services;

internal interface IDatabaseService<T>
{
    Task<List<T>> GetAllAsync(string? table = null);
    Task<T?> GetByIdAsync(string id, string? table = null);
    Task<T?> GetByNameAsync(string name, string? table = null);
    Task SaveAsync(T data, string? table = null);
}

public class BaseDatabaseModel<T> : IDatabaseService<T>
{
    private static string? _table = null;

    public BaseDatabaseModel(string table)
    {
        _table ??= table;
    }

    public async Task SaveAsync(T data, string? table=null)
    {
        var db = DbService.GetDb();
        var users = db.GetCollection<T>(table ?? _table);
        await users.InsertOneAsync(data);
    }
    public async Task<List<T>> GetAllAsync(string? table = null)
    {
        var db = DbService.GetDb();
        var data = db.GetCollection<T>(table ?? _table);
        var item = await data.FindAsync<T>(new BsonDocument());
        return item.ToList();
    }

    public async Task<T?> GetByIdAsync(string id, string? table= null)
    {
        var db = DbService.GetDb();
        var data = db.GetCollection<T>(table ?? _table);
        var filter = Builders<T>.Filter.Eq("Id", id);
        var item = await data.FindAsync<T>(filter);
        var list = item.ToList();
        return list.FirstOrDefault();
    }

    public async Task<T?> GetByNameAsync(string name, string? table = null)
    {
        var db = DbService.GetDb();
        var data = db.GetCollection<T>(table ?? _table);
        var filter = Builders<T>.Filter.Eq("Name", name);
        var item = await data.FindAsync<T>(filter);
        var list = item.ToList();
        return list.FirstOrDefault();
    }
}

