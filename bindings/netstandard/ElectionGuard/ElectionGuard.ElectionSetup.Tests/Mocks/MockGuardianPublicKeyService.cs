using ElectionGuard.ElectionSetup.Concurrency;
using ElectionGuard.Guardians;
using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver;

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

    public async Task UpdatePublicKeyAsync(
        string keyCeremonyId, string guardianId, ElectionPublicKey? key)
    {
        using (await _lock.LockAsync())
        {
            var record = Collection.Values.FirstOrDefault(
                    x => x.KeyCeremonyId!.Equals(keyCeremonyId)
                    && x.GuardianId!.Equals(guardianId));
            if (record == null)
            {
                throw new Exception($"Record not found {keyCeremonyId} {guardianId}");
            }

            Console.WriteLine(
                $"UpdatePublicKeyAsync update existing {keyCeremonyId} {guardianId} {record.Id}");
            record!.PublicKey = key != null ? new(key) : null;
            Collection[record.Id] = record;
        }
    }

    public override async Task<GuardianPublicKey> SaveAsync(
        GuardianPublicKey data, FilterDefinition<GuardianPublicKey>? customFilter = null, string? table = null)
    {
        using (await _lock.LockAsync())
        {
            data.Id ??= Guid.NewGuid().ToString();
            GuardianPublicKey record = new(data);
            record.Id = data.Id;
            Collection[record.Id] = record;
            return record;
        }
    }
}

