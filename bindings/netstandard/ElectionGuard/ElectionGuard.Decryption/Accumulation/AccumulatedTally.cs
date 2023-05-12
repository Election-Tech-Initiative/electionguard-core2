using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.Decryption.Accumulation;

/// <summary>
/// The Accumulated Decryption of a tally used when publishing the results of an election.
/// </summary>
public record AccumulatedTally
    : DisposableRecordBase, IEquatable<AccumulatedTally>
{
    /// <summary>
    /// The object id of the tally
    /// </summary>
    public string TallyId { get; init; }


    /// <summary>
    /// The accumulated partial decryptions of the contests of the tally
    /// </summary>
    public Dictionary<string, AccumulatedContest> Contests { get; init; } = default!;

    public AccumulatedTally(
        string tallyId,
        Dictionary<string, AccumulatedContest> contests)
    {
        TallyId = tallyId;
        Contests = contests;
    }

    public AccumulatedTally(
        string tallyId,
        List<AccumulatedContest> contests)
    {
        TallyId = tallyId;
        Contests = contests
            .Select(x => new AccumulatedContest(x))
            .ToDictionary(x => x.ObjectId);
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Contests.Dispose();
    }
}
