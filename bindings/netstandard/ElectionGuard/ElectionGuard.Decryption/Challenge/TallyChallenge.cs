using ElectionGuard.Ballot;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// The Decryption of a contest used when publishing the results of an election.
/// </summary>
public record TallyChallenge
    : DisposableRecordBase, IEquatable<TallyChallenge>
{
    // TODO: we probably need the tallyId here and in all the other objects where its missing

    /// <summary>
    /// The object id of the contest
    /// </summary>
    public string GuardianId { get; init; }

    // sequence order of the guardian
    public ulong SequenceOrder { get; init; }

    public ElementModQ Coefficient { get; init; }

    public Dictionary<string, ContestChallenge> Contests { get; init; } = new Dictionary<string, ContestChallenge>();

    public TallyChallenge(
        IElectionGuardian guardian,
        LagrangeCoefficient coefficient,
        InternalManifest manifest)
    {
        if (guardian.SequenceOrder != coefficient.SequenceOrder)
        {
            throw new ArgumentException(
                $"Guardian sequence order {guardian.SequenceOrder} does not match coefficient sequence order {coefficient.SequenceOrder}");
        }
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = coefficient.Coefficient;
        Contests = manifest.ToContestChallengeDictionary();
    }

    public TallyChallenge(
        IElectionGuardian guardian,
        ElementModQ coefficient,
        InternalManifest manifest)
    {
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = coefficient;
        Contests = manifest.ToContestChallengeDictionary();
    }

    public TallyChallenge(
        string guardianId,
        LagrangeCoefficient coefficient,
        InternalManifest manifest)
    {
        GuardianId = guardianId;
        SequenceOrder = coefficient.SequenceOrder;
        Coefficient = coefficient.Coefficient;
        Contests = manifest.ToContestChallengeDictionary();
    }

    public TallyChallenge(
        IElectionGuardian guardian,
        LagrangeCoefficient coefficient,
        Dictionary<string, ContestChallenge> contests)
    {
        if (guardian.SequenceOrder != coefficient.SequenceOrder)
        {
            throw new ArgumentException(
                $"Guardian sequence order {guardian.SequenceOrder} does not match coefficient sequence order {coefficient.SequenceOrder}");
        }
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = coefficient.Coefficient;
        Contests = contests;
    }

    public void Add(
        IElectionContest contest,
        SelectionChallenge selection)
    {
        Contests[contest.ObjectId].Add(selection);
    }

    public void Add(
        IElectionContest contest,
        IElectionSelection selection,
        ElementModQ challenge)
    {
        Contests[contest.ObjectId].Add(new SelectionChallenge(
            selection.ObjectId,
            GuardianId,
            SequenceOrder,
            Coefficient,
            challenge));
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
    }
}

public static partial class InternalManifestExtensions
{
    /// <summary>
    /// Converts an internal manifest to a dictionary of CiphertextTallyContest
    /// </summary>
    public static Dictionary<string, ContestChallenge> ToContestChallengeDictionary(
        this InternalManifest manifest)
    {
        var contests = new Dictionary<string, ContestChallenge>();
        foreach (var contest in manifest.Contests)
        {
            contests.Add(
                contest.ObjectId,
                new ContestChallenge(contest));
        }
        return contests;
    }
}
