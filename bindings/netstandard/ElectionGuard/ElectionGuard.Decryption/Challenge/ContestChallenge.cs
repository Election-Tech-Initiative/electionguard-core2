using ElectionGuard.Ballot;
using ElectionGuard.Base;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// The Decryption of a contest used when publishing the results of an election.
/// </summary>
public record ContestChallenge
    : DisposableRecordBase, IElectionObject, IEquatable<ContestChallenge>
{
    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string ObjectId { get; init; }

    public ulong SequenceOrder { get; init; }

    public Dictionary<string, SelectionChallenge> Selections { get; init; } = new Dictionary<string, SelectionChallenge>();

    public ContestChallenge(
        string objectId, ulong sequenceOrder)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
    }

    public ContestChallenge(
        IElectionContest contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
    }

    public ContestChallenge(
        string objectId,
        ulong sequenceOrder,
        Dictionary<string, SelectionChallenge> selections)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
        Selections = selections;
    }

    public ContestChallenge(
        IElectionContest contest,
        Dictionary<string, SelectionChallenge> selections)
    {
        ObjectId = contest.ObjectId;
        Selections = selections;
    }

    public void Add(SelectionChallenge selection)
    {
        Selections.Add(selection.ObjectId, selection);
    }


    protected override void DisposeUnmanaged()
    {
        Selections.Dispose();
        base.DisposeUnmanaged();
    }
}
