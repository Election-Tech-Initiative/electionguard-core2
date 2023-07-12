using ElectionGuard.Ballot;
using ElectionGuard.Decryption.Accumulation;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// A challenge for a ballot that is scoped to a specific guardian
/// </summary>
public record BallotChallenge
    : DisposableRecordBase, IEquatable<BallotChallenge>
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
    /// The object id of the guardian
    /// </summary>
    public string GuardianId { get; init; }

    /// <summary>
    /// The sequence order of the guardian
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The lagrange coefficient of the guardian
    /// </summary>
    public ElementModQ Coefficient { get; init; }

    public Dictionary<string, ContestChallenge> Contests { get; init; } = default!;

    public BallotChallenge(
        string tallyId,
        IElectionGuardian guardian,
        LagrangeCoefficient coefficient,
        CiphertextBallot ballot,
        InternalManifest manifest)
    {
        if (guardian.SequenceOrder != coefficient.SequenceOrder)
        {
            throw new ArgumentException(
                $"Guardian sequence order {guardian.SequenceOrder} does not match coefficient sequence order {coefficient.SequenceOrder}");
        }
        TallyId = tallyId;
        ObjectId = ballot.ObjectId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = new(coefficient.Coefficient);
        Contests = manifest
            .GetContests(ballot.StyleId)
            .ToContestChallengeDictionary();
    }

    public BallotChallenge(
        string tallyId,
        IElectionGuardian guardian,
        LagrangeCoefficient coefficient,
        CiphertextBallot ballot)
    {
        if (guardian.SequenceOrder != coefficient.SequenceOrder)
        {
            throw new ArgumentException(
                $"Guardian sequence order {guardian.SequenceOrder} does not match coefficient sequence order {coefficient.SequenceOrder}");
        }
        TallyId = tallyId;
        ObjectId = ballot.ObjectId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = new(coefficient.Coefficient);
        Contests = ballot
            .ToContestChallengeDictionary();
    }

    [JsonConstructor]
    public BallotChallenge(
        string tallyId,
        string objectId,
        string guardianId,
        ulong sequenceOrder,
        ElementModQ coefficient,
        Dictionary<string, ContestChallenge> contests)
    {
        TallyId = tallyId;
        ObjectId = objectId;
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;
        Coefficient = new(coefficient);
        Contests = contests
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new ContestChallenge(kvp.Value));
    }


    public BallotChallenge(
        string tallyId,
        IElectionGuardian guardian,
        ElementModQ coefficient,
        AccumulatedBallot accumulated)
    {
        TallyId = tallyId;
        ObjectId = accumulated.ObjectId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = new(coefficient);
        Contests = accumulated
            .ToContestChallengeDictionary();
    }

    public BallotChallenge(
        string tallyId,
        string guardianId,
        LagrangeCoefficient coefficient,
        CiphertextBallot ballot)
    {
        TallyId = tallyId;
        ObjectId = ballot.ObjectId;
        GuardianId = guardianId;
        SequenceOrder = coefficient.SequenceOrder;
        Coefficient = new(coefficient.Coefficient);
        Contests = ballot
            .ToContestChallengeDictionary();
    }

    public BallotChallenge(
        string tallyId,
        string guardianId,
        LagrangeCoefficient coefficient,
        AccumulatedBallot accumulated)
    {
        TallyId = tallyId;
        ObjectId = accumulated.ObjectId;
        GuardianId = guardianId;
        SequenceOrder = coefficient.SequenceOrder;
        Coefficient = new(coefficient.Coefficient);
        Contests = accumulated
            .ToContestChallengeDictionary();
    }

    public BallotChallenge(BallotChallenge other) : base(other)
    {
        TallyId = other.TallyId;
        ObjectId = other.ObjectId;
        GuardianId = other.GuardianId;
        SequenceOrder = other.SequenceOrder;
        Coefficient = new(other.Coefficient);
        Contests = other.Contests
            .Select(x => new ContestChallenge(x.Value))
            .ToDictionary(x => x.ObjectId);
    }

    /// <summary>
    /// Add an existing selection challenge to the contest collection
    /// </summary>
    public void Add(
        IElectionContest contest,
        SelectionChallenge selection)
    {
        Contests[contest.ObjectId].Add(selection);
    }

    /// <summary>
    /// Add a new selection challenge to the contest collection
    /// </summary>
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

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Contests.Dispose();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Coefficient?.Dispose();
    }
}

public static partial class InternalManifestExtensions
{
    /// <summary>
    /// Converts an internal manifest to a dictionary of CiphertextTallyContest
    /// </summary>
    public static Dictionary<string, ContestChallenge> ToContestChallengeDictionary(
        this List<ContestDescriptionWithPlaceholders> contestDescriptions)
    {
        var contests = new Dictionary<string, ContestChallenge>();
        foreach (var contestDescription in contestDescriptions)
        {
            contests.Add(
                contestDescription.ObjectId,
                new ContestChallenge(contestDescription));
        }
        return contests;
    }
}

public static partial class CiphertextBallotExtensions
{
    /// <summary>
    /// Converts an internal manifest to a dictionary of CiphertextTallyContest
    /// </summary>
    public static Dictionary<string, ContestChallenge> ToContestChallengeDictionary(
        this CiphertextBallot ballot)
    {
        var contests = new Dictionary<string, ContestChallenge>();
        foreach (var contest in ballot.Contests)
        {
            contests.Add(
                contest.ObjectId,
                new ContestChallenge(contest));
        }
        return contests;
    }
}

public static partial class AccumulatedBallotExtensions
{
    /// <summary>
    /// Converts an internal manifest to a dictionary of CiphertextTallyContest
    /// </summary>
    public static Dictionary<string, ContestChallenge> ToContestChallengeDictionary(
        this AccumulatedBallot accumulated)
    {
        var contests = new Dictionary<string, ContestChallenge>();
        foreach (var (contestId, contest) in accumulated.Contests)
        {
            contests.Add(
                contest.ObjectId,
                new ContestChallenge(contest));
        }
        return contests;
    }
}


