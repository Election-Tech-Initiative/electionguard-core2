
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Ballot;
using ElectionGuard.Base;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ChallengeResponse;

/// <summary>
/// A response to a challenge for a specific contest
/// </summary>
public record ContestChallengeResponse
: DisposableRecordBase, IElectionObject, IEquatable<ContestChallengeResponse>
{
    /// <summary>
    /// The object id of the selection.
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The sequence order of the guardian
    /// </summary>
    public ulong SequenceOrder { get; init; }

    public Dictionary<string, SelectionChallengeResponse> Selections { get; init; } = new Dictionary<string, SelectionChallengeResponse>();

    public ContestChallengeResponse(
        string objectId, ulong sequenceOrder)
    {
        ObjectId = objectId;
        SequenceOrder = sequenceOrder;
    }

    public ContestChallengeResponse(
        ContestChallenge contest)
    {
        ObjectId = contest.ObjectId;
        SequenceOrder = contest.SequenceOrder;
    }

    public ContestChallengeResponse(ContestChallengeResponse other) : base(other)
    {
        ObjectId = other.ObjectId;
        SequenceOrder = other.SequenceOrder;
        Selections = other.Selections
            .ToDictionary(x => x.Key, x => new SelectionChallengeResponse(x.Value));
    }

    /// <summary>
    /// Adds a selection challenge response to the contest response
    /// </summary>
    public void Add(SelectionChallengeResponse selection)
    {
        Selections.Add(selection.ObjectId, selection);
    }

    public bool IsValid(
        ICiphertextContest contest,
        ElectionPublicKey guardian,
        ContestShare share,
        ContestChallenge challenge
    )
    {
        foreach (var (selectionId, selection) in Selections)
        {
            if (!selection.IsValid(
                contest.Selections.First(x => x.ObjectId == selectionId),
                guardian,
                share.Selections[selectionId],
                challenge.Selections[selectionId]
            ))
            {
                Console.WriteLine($"ContestChallengeResponse: Invalid selection {selectionId}");
                return false;
            }
        }

        return true;
    }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Selections.Dispose();
    }
}
