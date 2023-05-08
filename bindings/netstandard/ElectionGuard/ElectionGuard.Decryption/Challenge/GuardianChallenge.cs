using ElectionGuard.ElectionSetup;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Challenge;

/// <summary>
/// A challenge for a specific guardian to prove that a decryption share was generated correctly.
/// </summary>
public record GuardianChallenge : DisposableRecordBase
{
    public string GuardianId { get; init; }

    public ulong SequenceOrder { get; init; }

    public TallyChallenge Tally { get; init; }

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


    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Tally.Dispose();
        // TODO Ballots.Dispose();
    }
}
