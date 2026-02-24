using ElectionGuard.ElectionSetup.Concurrency;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver;

namespace ElectionGuard.ElectionSetup.Tests.Mocks;

public class MockKeyCeremonyService : MockBaseDatabaseServiceBase<KeyCeremonyRecord>, IKeyCeremonyService
{
    private readonly AsyncLock _lock = new();

    public Task<List<KeyCeremonyRecord>?> GetAllCompleteAsync()
    {
        throw new NotImplementedException();
    }

    public Task<List<KeyCeremonyRecord>?> GetAllNotCompleteAsync()
    {
        throw new NotImplementedException();
    }

    public Task<KeyCeremonyRecord?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        return Task.FromResult(Collection[keyCeremonyId] ?? null);
    }

    public Task UpdateCompleteAsync(string keyCeremonyId, ElectionJointKey jointKey)
    {
        var record = Collection[keyCeremonyId];
        record.JointKey = jointKey;
        record.State = KeyCeremonyState.Complete;
        record.CompletedAt = DateTime.Now;
        record.UpdatedAt = DateTime.Now;
        Collection[keyCeremonyId] = record;
        return Task.CompletedTask;
    }

    public async Task UpdateStateAsync(string keyCeremonyId, KeyCeremonyState state)
    {
        using (await _lock.LockAsync())
        {
            Console.WriteLine($"UpdateStateAsync {keyCeremonyId} {state}");
            var record = Collection[keyCeremonyId];
            record.State = state;
            record.UpdatedAt = DateTime.Now;
            Collection[keyCeremonyId] = record;
        }
    }
    public override async Task<KeyCeremonyRecord> SaveAsync(KeyCeremonyRecord data, FilterDefinition<KeyCeremonyRecord>? customFilter = null, string? table = null)
    {
        using (await _lock.LockAsync())
        {
            data.Id ??= Guid.NewGuid().ToString();
            KeyCeremonyRecord record = new(data);
            record.Id = data.Id;
            Collection[record.Id] = record;
            return data;
        }
    }

}
