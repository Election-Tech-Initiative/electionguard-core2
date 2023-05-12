using ElectionGuard.ElectionSetup;
using ElectionGuard.Extensions;
using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.Decryption.Accumulation;

/// <summary>
/// The Accumulated Decryption of a ballot used when publishing the results of an election.
/// </summary>
public record AccumulatedBallot
    : DisposableRecordBase, IEquatable<AccumulatedBallot>
{
    /// <summary>
    /// The object id of the tally
    /// </summary>
    public string TallyId { get; init; }

    /// <summary>
    /// The object id of the ballot
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The accumulated partial decryptions of the contests of the ballot
    /// </summary>
    public Dictionary<string, AccumulatedContest> Contests { get; init; } = default!;

    public AccumulatedBallot(
        string tallyId,
        string objectId,
        Dictionary<string, AccumulatedContest> contests)
    {
        TallyId = tallyId;
        ObjectId = objectId;
        Contests = contests
            .Select(x => new AccumulatedContest(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    public AccumulatedBallot(
        string tallyId,
        string objectId,
        List<AccumulatedContest> contests)
    {
        TallyId = tallyId;
        ObjectId = objectId;
        Contests = contests
            .Select(x => new AccumulatedContest(x))
            .ToDictionary(x => x.ObjectId);
    }

    public AccumulatedBallot(AccumulatedBallot other) : base(other)
    {
        TallyId = other.TallyId;
        ObjectId = other.ObjectId;
        Contests = other.Contests
            .Select(x => new AccumulatedContest(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Contests.Dispose();
    }
}
