using ElectionGuard.ElectionSetup;
using ElectionGuard.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// A challenge for a specific guardian to prove that a decryption share was generated correctly.
/// </summary>
public record GuardianChallenge : DisposableRecordBase
{
    /// <summary>
    /// The object id of the guardian.
    /// </summary>
    public string GuardianId { get; init; }

    /// <summary>
    /// The sequence order of the guardian.
    /// </summary>
    public ulong SequenceOrder { get; init; }

    /// <summary>
    /// The tally challenge for this guardian.
    /// </summary>
    public TallyChallenge Tally { get; init; }

    /// <summary>
    /// The ballot challenges for this guardian.
    /// </summary>
    public List<BallotChallenge> Ballots { get; init; }

    public GuardianChallenge(
        IElectionGuardian guardian,
        TallyChallenge tally,
        List<BallotChallenge>? ballots)
    {
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Tally = new(tally);
        Ballots = ballots != null
            ? ballots.Select(x => new BallotChallenge(x)).ToList() : new();
    }

    [JsonConstructor]
    public GuardianChallenge(
        string guardianId,
        ulong sequenceOrder,
        TallyChallenge tally,
        List<BallotChallenge>? ballots)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;
        Tally = new(tally);
        Ballots = ballots != null
            ? ballots.Select(x => new BallotChallenge(x)).ToList() : new();
    }


    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Tally.Dispose();
        Ballots.Dispose();
    }
}
