using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Challenge;
using ElectionGuard.Decryption.Shares;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ChallengeResponse;

/// <summary>
/// The Challenge response for a specific tally
/// </summary>
public record TallyChallengeResponse
    : DisposableRecordBase, IEquatable<TallyChallengeResponse>
{
    // TODO: tallyId

    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string GuardianId { get; init; }

    /// <summary>
    /// Sequence order of the guardian
    /// </summary>
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

    public TallyChallengeResponse(TallyChallengeResponse other) : base(other)
    {
        GuardianId = other.GuardianId;
        SequenceOrder = other.SequenceOrder;
        Coefficient = other.Coefficient;
        Contests = other.Contests
            .ToDictionary(x => x.Key, x => new ContestChallengeResponse(x.Value));
    }

    public void Add(
        IElectionContest contest,
        SelectionChallengeResponse selection)
    {
        Contests[contest.ObjectId].Add(selection);
    }

    public bool IsValid(
        CiphertextTally tally,
        ElectionPublicKey guardian,
        TallyShare share,
        TallyChallenge challenge)
    {
        foreach (var (contestId, contest) in Contests)
        {
            if (!contest.IsValid(
                tally.Contests[contestId],
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

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Contests.Dispose();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Coefficient.Dispose();
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


