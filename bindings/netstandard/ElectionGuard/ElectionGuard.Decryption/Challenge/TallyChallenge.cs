using ElectionGuard.Ballot;
using ElectionGuard.ElectionSetup;
using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// A challenge for a specific tally that is scoped to a specific guardian.
/// </summary>
public record TallyChallenge
    : DisposableRecordBase, IEquatable<TallyChallenge>
{
    /// <summary>
    /// The object id of the tally
    /// </summary>
    public string TallyId { get; init; }

    /// <summary>
    /// The object id of the guardian
    /// </summary>
    public string GuardianId { get; init; }

    /// <summary>
    /// sequence order of the guardian
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// lagrange coefficient of the guardian
    /// </summary>
    public ElementModQ Coefficient { get; init; }

    public Dictionary<string, ContestChallenge> Contests { get; init; } = new Dictionary<string, ContestChallenge>();

    public TallyChallenge(
        string tallyId,
        IElectionGuardian guardian,
        LagrangeCoefficient coefficient,
        InternalManifest manifest)
    {
        if (guardian.SequenceOrder != coefficient.SequenceOrder)
        {
            throw new ArgumentException(
                $"Guardian sequence order {guardian.SequenceOrder} does not match coefficient sequence order {coefficient.SequenceOrder}");
        }
        TallyId = tallyId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = coefficient.Coefficient;
        Contests = manifest.ToContestChallengeDictionary();
    }

    public TallyChallenge(
        string tallyId,
        IElectionGuardian guardian,
        ElementModQ coefficient,
        InternalManifest manifest)
    {
        TallyId = tallyId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = coefficient;
        Contests = manifest.ToContestChallengeDictionary();
    }

    [JsonConstructor]
    public TallyChallenge(
        string tallyId,
        string guardianId,
        ulong sequenceOrder,
        ElementModQ coefficient,
        Dictionary<string, ContestChallenge> contests)
    {
        TallyId = tallyId;
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;
        Coefficient = coefficient;
        Contests = contests
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new ContestChallenge(kvp.Value));
    }

    public TallyChallenge(
        string tallyId,
        string guardianId,
        LagrangeCoefficient coefficient,
        InternalManifest manifest)
    {
        TallyId = tallyId;
        GuardianId = guardianId;
        SequenceOrder = coefficient.SequenceOrder;
        Coefficient = coefficient.Coefficient;
        Contests = manifest.ToContestChallengeDictionary();
    }

    public TallyChallenge(
        string tallyId,
        IElectionGuardian guardian,
        LagrangeCoefficient coefficient,
        Dictionary<string, ContestChallenge> contests)
    {
        if (guardian.SequenceOrder != coefficient.SequenceOrder)
        {
            throw new ArgumentException(
                $"Guardian sequence order {guardian.SequenceOrder} does not match coefficient sequence order {coefficient.SequenceOrder}");
        }
        TallyId = tallyId;
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Coefficient = new(coefficient.Coefficient);
        Contests = contests
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new ContestChallenge(kvp.Value));
    }

    public TallyChallenge(TallyChallenge other) : base(other)
    {
        TallyId = other.TallyId;
        GuardianId = other.GuardianId;
        SequenceOrder = other.SequenceOrder;
        Coefficient = new(other.Coefficient);
        Contests = other.Contests
            .ToDictionary(
                kvp => kvp.Key,
                kvp => new ContestChallenge(kvp.Value));
    }

    /// <summary>
    /// Add an existing Selection challenge for a contest to the tally challenge
    /// </summary>
    public void Add(
        IElectionContest contest,
        SelectionChallenge selection)
    {
        Contests[contest.ObjectId].Add(selection);
    }

    /// <summary>
    /// Add a new Selection challenge for a contest to the tally challenge
    /// </summary>
    public void Add(
        IElectionContest contest,
        IElectionSelection selection,
        ElementModQ challenge)
    {
        Contests[contest.ObjectId].Add(
            new SelectionChallenge(
                selection.ObjectId,
                GuardianId,
                SequenceOrder,
                Coefficient,
                challenge
            )
        );
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
