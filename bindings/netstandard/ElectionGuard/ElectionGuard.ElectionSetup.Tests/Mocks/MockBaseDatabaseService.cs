using System.Collections.Concurrent;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver;

namespace ElectionGuard.ElectionSetup.Tests.Mocks;

public abstract class MockBaseDatabaseServiceBase<T> : DisposableBase, IDatabaseService<T> where T : DatabaseRecord
{
    protected readonly ConcurrentDictionary<string, T> Collection = new();

    public Task<long> CountByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        throw new NotImplementedException();
    }

    public Task<bool> ExistsByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetAllAsync(string? table = null)
    {
        return Task.FromResult(Collection.Values.ToList());
    }

    public Task<List<T>> GetAllByFieldAsync(string fieldName, object fieldValue, string? table = null)
    {
        throw new NotImplementedException();
    }

    public Task<List<T>> GetAllByFilterAsync(FilterDefinition<T> filter, string? table = null)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByFieldAsync(string fieldName, object fieldValue, string? table = null)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByIdAsync(string id, string? table = null)
    {
        throw new NotImplementedException();
    }

    public Task<T?> GetByNameAsync(string name, string? table = null)
    {
        throw new NotImplementedException();
    }

    public abstract Task<T> SaveAsync(T data, FilterDefinition<T>? customFilter = null, string? table = null);

    public Task UpdateAsync(FilterDefinition<T> filter, UpdateDefinition<T> update, string? table = null)
    {
        throw new NotImplementedException();
    }

    public FilterDefinition<T> UpdateFilter(FilterDefinition<T> filter, bool getDeleted = false)
    {
        throw new NotImplementedException();
    }
}
