using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

public class TallyJoinedService : BaseDatabaseService<TallyJoinedRecord>
{
    private readonly static string _collection = DbConstants.TableTallies;

    public TallyJoinedService() : base(_collection, nameof(TallyJoinedRecord))
    {
    }

    public async Task<List<TallyJoinedRecord>> GetAllByTallyIdAsync(string tallyId) => 
        await GetAllByFieldAsync(DbConstants.TallyId, tallyId);

    public async Task JoinTallyAsync(TallyJoinedRecord joiner)
    {
        var filter = FilterBuilder.And(
                FilterBuilder.Eq(DbConstants.TallyId, joiner.TallyId),
                FilterBuilder.Eq(DbConstants.GuardianId, joiner.GuardianId));

        var count = await CountByFilterAsync(filter);
        if (count == 0)
        {
            _ = await SaveAsync(joiner);
        }
    }

    public async Task<List<string>> GetGuardianRejectedIdsAsync(string guardianId)
    {
        var filter = FilterBuilder.And(
                FilterBuilder.Eq(DbConstants.Joined, false),
                FilterBuilder.Eq(DbConstants.GuardianId, guardianId));

        var list = await GetAllByFilterAsync(filter);
        return list.Select(t => t.TallyId!).ToList();
    }

    /// <summary>
    /// Get counts of guardian participation and consent to be part of a tally
    /// </summary>
    public async Task<Dictionary<bool, int>> GetGuardianCountByTallyAsync(string tallyId)
    {
        var filter = FilterBuilder.Eq(DbConstants.TallyId, tallyId);

        List<TallyJoinedRecord> guardiansParticipating = await GetAllByFilterAsync(filter) ?? new();
        return guardiansParticipating.GroupBy(g => g.Joined).ToDictionary(g => g.Key, g => g.Count());
    }

    public async Task<long> GetCountByTallyJoinedAsync(string tallyId)
    {
        var filter = FilterBuilder.And(
                FilterBuilder.Eq(DbConstants.TallyId, tallyId),
                FilterBuilder.Eq(DbConstants.Joined, true));

        return await CountByFilterAsync(filter);
    }

    public async Task<bool> GetExistsByTallyAsync(string tallyId, string guardianId)
    {
        var filter = FilterBuilder.And(
            FilterBuilder.Eq(DbConstants.TallyId, tallyId),
            FilterBuilder.Eq(DbConstants.GuardianId, guardianId)
            );
        return await ExistsByFilterAsync(filter);
    }

}
