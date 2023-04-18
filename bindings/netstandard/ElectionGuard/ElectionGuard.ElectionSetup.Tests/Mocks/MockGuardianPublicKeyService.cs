using ElectionGuard.ElectionSetup.Concurrency;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;

namespace ElectionGuard.ElectionSetup.Tests.Mocks;

public class MockGuardianPublicKeyService : MockBaseDatabaseServiceBase<GuardianPublicKey>, IGuardianPublicKeyService
{
    private readonly AsyncLock _lock = new();

    public Task<long> CountAsync(string keyCeremonyId)
    {
        var count = Collection.Values.Count(x => x.KeyCeremonyId == keyCeremonyId);
        return Task.FromResult((long)count);
    }

    public Task<List<GuardianPublicKey>> GetAllByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        var records = Collection.Values.Where(
            x => x.KeyCeremonyId == keyCeremonyId).ToList();
        return Task.FromResult(records);
    }

    public Task<GuardianPublicKey?> GetByIdsAsync(string keyCeremonyId, string guardianId)
    {
        var record = Collection.Values.FirstOrDefault
        (x => x.KeyCeremonyId == keyCeremonyId && x.GuardianId == guardianId);
        return Task.FromResult(record);
    }

    public async Task UpdatePublicKeyAsync(string keyCeremonyId, string guardianId, ElectionPublicKey? key)
    {
        using (await _lock.LockAsync())
        {
            var record = Collection.Values.FirstOrDefault(
                x => x.KeyCeremonyId == keyCeremonyId
                && x.GuardianId == guardianId);
            if (record != null)
            {
                Console.WriteLine(
                    $"UpdatePublicKeyAsync update existing {keyCeremonyId} {guardianId}");
                record.PublicKey = key != null ? new(key) : null;
                Collection[record.Id] = record;
            }
            else
            {
                Console.WriteLine(
                    $"UpdatePublicKeyAsync create new {keyCeremonyId} {guardianId}");
                var newRecord = new GuardianPublicKey
                {
                    KeyCeremonyId = keyCeremonyId,
                    GuardianId = guardianId,
                    PublicKey = key != null ? new(key) : null
                };
                Collection[newRecord.Id] = newRecord;
            }

        }
    }

    public override Task<GuardianPublicKey> SaveAsync(GuardianPublicKey data, string? table = null)
    {
        throw new NotImplementedException();
    }
}
