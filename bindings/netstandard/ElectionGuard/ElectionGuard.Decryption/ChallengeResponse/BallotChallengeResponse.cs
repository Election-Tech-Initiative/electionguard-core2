using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ChallengeResponse;

/// <summary>
/// The Decryption of a contest used when publishing the results of an election.
/// </summary>
public record BallotChallengeResponse
    : DisposableRecordBase, IEquatable<BallotChallengeResponse>
{
    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string ObjectId { get; init; }

    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string GuardianId { get; init; }

    // sequence order of the guardian
    public ulong SequenceOrder { get; init; }

    public ElementModQ Coefficient { get; init; }

    public Dictionary<string, ContestChallengeResponse> Contests { get; init; } = default!;

    public BallotChallengeResponse(
        BallotChallenge challenge)
    {
        ObjectId = challenge.ObjectId;
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
        CiphertextBallot ballot,
        ElectionPublicKey guardian,
        BallotShare share,
        BallotChallenge challenge)
    {
        foreach (var (contestId, contest) in Contests)
        {
            if (!contest.IsValid(
                ballot.Contests.First(x => x.ObjectId == contestId),
                guardian,
                share.Contests[contestId],
                challenge.Contests[contestId]
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

public static partial class BallotChallengeExtensions
{
    /// <summary>
    /// Converts an internal manifest to a dictionary of CiphertextTallyContest
    /// </summary>
    public static Dictionary<string, ContestChallengeResponse> ToContestChallengeResponseDictionary(
        this BallotChallenge accumulated)
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


