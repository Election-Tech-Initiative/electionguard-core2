
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Ballot;
using ElectionGuard.Base;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ChallengeResponse;
public record ContestChallengeResponse
: DisposableRecordBase, IElectionObject, IEquatable<ContestChallengeResponse>
{
    /// <summary>
    /// The object id of the selection.
    /// </summary>
    public string ObjectId { get; init; }

    // the sequence order of the guardian
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
}
