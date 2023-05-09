using ElectionGuard.UI.Lib.Models;

namespace ElectionGuard.UI.Lib.Services;

public class TallyJoinedService : BaseDatabaseService<TallyJoinedRecord>
{
    private readonly static string _collection = Constants.TableTallies;

    public TallyJoinedService() : base(_collection, nameof(TallyJoinedRecord))
    {
    }

    public async Task<List<TallyJoinedRecord>> GetAllByTallyIdAsync(string tallyId) => 
        await GetAllByFieldAsync(Constants.TallyId, tallyId);

    public async Task JoinTally(TallyJoinedRecord joiner)
    {
        var filter = FilterBuilder.And(
                FilterBuilder.Eq(Constants.TallyId, joiner.TallyId),
                FilterBuilder.Eq(Constants.GuardianId, joiner.GuardianId));

        var count = await CountByFilterAsync(filter);
        if (count == 0)
        {
            _ = await SaveAsync(joiner);
        }
    }
}
