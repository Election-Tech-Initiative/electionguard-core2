using ElectionGuard.UI.Lib.Models;
using ElectionGuard.UI.Lib.Services;
using MongoDB.Driver;

namespace ElectionGuard.ElectionSetup.Tests.Mocks;

public class MockVerificationService : MockBaseDatabaseServiceBase<ElectionPartialKeyVerification>, IVerificationService
{
    public Task<long> CountAsync(string keyCeremonyId, string guardianId)
    {
        var count = Collection.Values.Count(x => x.KeyCeremonyId == keyCeremonyId && x.DesignatedId == guardianId);
        return Task.FromResult((long)count);
    }

    public Task<long> CountAsync(string keyCeremonyId)
    {
        var count = Collection.Values.Count(x => x.KeyCeremonyId == keyCeremonyId);
        return Task.FromResult((long)count);
    }

    public Task<List<ElectionPartialKeyVerification>?> GetAllByKeyCeremonyIdAsync(string keyCeremonyId)
    {
        var records = Collection.Values.Where(x => x.KeyCeremonyId == keyCeremonyId).ToList();
        return Task.FromResult(records ?? null);
    }

    public override Task<ElectionPartialKeyVerification> SaveAsync(ElectionPartialKeyVerification data, FilterDefinition<ElectionPartialKeyVerification>? customFilter = null, string? table = null)
    {
        data.Id ??= Guid.NewGuid().ToString();
        ElectionPartialKeyVerification record = new(data);
        record.Id = data.Id;
        Collection[record.Id] = record;
        return Task.FromResult(data);
    }
}
