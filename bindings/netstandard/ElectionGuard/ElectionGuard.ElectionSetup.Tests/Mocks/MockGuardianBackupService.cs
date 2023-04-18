using System.Text.Json;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.ElectionSetup.Tests.Mocks;

public class MockGuardianBackupService : MockBaseDatabaseServiceBase<GuardianBackups>, IGuardianBackupService
{
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

    public override Task<GuardianBackups> SaveAsync(GuardianBackups data, string? table = null)
    {
        //Console.WriteLine($"MockGuardianBackupService.SaveAsync {JsonSerializer.Serialize(data.Backup)}");
        data.Id ??= Guid.NewGuid().ToString();
        Collection[data.Id] = new(data);
        return Task.FromResult(data);
    }

}
