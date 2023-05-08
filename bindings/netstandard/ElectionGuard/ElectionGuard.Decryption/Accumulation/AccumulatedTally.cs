using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.Accumulation;

/// <summary>
/// The Accumulated Decryption of a ballot used when publishing the results of an election.
/// </summary>
public record AccumulatedTally
    : DisposableRecordBase, IEquatable<AccumulatedTally>
{
    /// <summary>
    /// The object id of the tally
    /// </summary>
    public string ObjectId { get; init; }


    public Dictionary<string, AccumulatedContest> Contests { get; init; } = default!;

    public AccumulatedTally(
        string objectId,
        Dictionary<string, AccumulatedContest> contests)
    {
        ObjectId = objectId;
        Contests = contests;
    }

    public AccumulatedTally(
        string objectId,
        List<AccumulatedContest> contests)
    {
        ObjectId = objectId;
        Contests = contests.ToDictionary(x => x.ObjectId, x => x);
    }


    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
    }
}
