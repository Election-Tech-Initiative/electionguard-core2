using ElectionGuard.ElectionSetup.Concurrency;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver;

namespace ElectionGuard.ElectionSetup.Tests.Mocks;

public class MockGuardianBackupService : MockBaseDatabaseServiceBase<GuardianBackups>, IGuardianBackupService
{
    private readonly AsyncLock _lock = new();
    public Task<long> CountAsync(string keyCeremonyId)
    {
        var count = Collection.Values.Count(x => x.KeyCeremonyId == keyCeremonyId);
        return Task.FromResult((long)count);
    }

    public Task<long> CountAsync(string keyCeremonyId, string guardianId)
    {
        var count = Collection.Values.Count(x => x.KeyCeremonyId == keyCeremonyId
        && x.GuardianId == guardianId);
        return Task.FromResult((long)count);
    }

    public Task<List<GuardianBackups>?> GetByGuardianIdAsync(string keyCeremonyId, string DesignatedId)
    {
        var backups = Collection.Values.Where(
            x => x.KeyCeremonyId == keyCeremonyId
            && x.DesignatedId == DesignatedId).ToList();
        return Task.FromResult(backups ?? null);
    }

    public Task<List<GuardianBackups>?> GetByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        var backups = Collection.Values.Where(x => x.KeyCeremonyId == keyCeremonyId).ToList();
        return Task.FromResult(backups ?? null);
    }

    public override async Task<GuardianBackups> SaveAsync(
        GuardianBackups data, FilterDefinition<GuardianBackups>? customFilter = null, string? table = null)
    {
        using (await _lock.LockAsync())
        {
            Console.WriteLine($"MockGuardianBackupService.SaveAsync {data.GuardianId} -> {data.DesignatedId}");
            data.Id ??= Guid.NewGuid().ToString();
            GuardianBackups record = new(data);
            record.Id = data.Id;
            Collection[record.Id] = record;
            return record;
        }
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        _lock.Dispose();
    }
}
