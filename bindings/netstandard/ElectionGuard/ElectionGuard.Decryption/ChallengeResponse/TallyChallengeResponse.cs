using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.ChallengeResponse;

/// <summary>
/// The Decryption of a contest used when publishing the results of an election.
/// </summary>
public record TallyChallengeResponse
    : DisposableRecordBase, IEquatable<TallyChallengeResponse>
{
    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string GuardianId { get; init; }

    // sequence order of the guardian
    public ulong SequenceOrder { get; init; }

    public ElementModQ Coefficient { get; init; }

    public Dictionary<string, ContestChallengeResponse> Contests { get; init; } = default!;

    public TallyChallengeResponse(
        TallyChallenge challenge)
    {
        GuardianId = challenge.GuardianId;
        SequenceOrder = challenge.SequenceOrder;
        Coefficient = challenge.Coefficient;
        Contests = challenge
            .ToContestChallengeResponseDictionary();
    }

    public void Add(
        IElectionContest contest,
        SelectionChallengeResponse selection)
    {
        Contests[contest.ObjectId].Add(selection);
    }

    public bool IsValid(
        CiphertextTally tally,
        AccumulatedTally accumulated,
        TallyShare share,
        TallyChallenge challenge)
    {
        foreach (var (contestId, contest) in Contests)
        {
            if (!contest.IsValid(
                tally.Contests[contestId],
                accumulated.Contests[contestId],
                share.Contests[contestId],
                challenge.Contests[contestId],
                tally.Context
            ))
            {
                return false;
            }
        }

        return true;
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
    }
}

public static partial class TallyChallengeExtensions
{
    /// <summary>
    /// Converts an internal manifest to a dictionary of CiphertextTallyContest
    /// </summary>
    public static Dictionary<string, ContestChallengeResponse> ToContestChallengeResponseDictionary(
        this TallyChallenge accumulated)
    {
        var contests = new Dictionary<string, ContestChallengeResponse>();
        foreach (var (contestId, contest) in accumulated.Contests)
        {
            contests.Add(
                contest.ObjectId,
                new ContestChallengeResponse(contest));
        }
        return contests;
    }
}


