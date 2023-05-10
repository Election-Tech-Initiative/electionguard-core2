using ElectionGuard.ElectionSetup;
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
        Tally = tally;
        Ballots = ballots ?? new List<BallotChallenge>();
    }


    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Tally.Dispose();
        foreach (var ballot in Ballots)
        {
            ballot.Dispose();
        }
    }
}
