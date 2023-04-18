using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.ElectionSetup.Tests.Mocks;

public class MockKeyCeremonyService : MockBaseDatabaseServiceBase<KeyCeremonyRecord>, IKeyCeremonyService
{
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

    public Task UpdateStateAsync(string keyCeremonyId, KeyCeremonyState state)
    {
        Console.WriteLine($"UpdateStateAsync {keyCeremonyId} {state}");
        var record = Collection[keyCeremonyId];
        record.State = state;
        record.UpdatedAt = DateTime.Now;
        Collection[keyCeremonyId] = record;
        return Task.CompletedTask;
    }

    public override Task<KeyCeremonyRecord> SaveAsync(KeyCeremonyRecord data, string? table = null)
    {
        data.Id ??= Guid.NewGuid().ToString();
        Collection[data.Id] = new(data);
        return Task.FromResult(data);
    }
}
