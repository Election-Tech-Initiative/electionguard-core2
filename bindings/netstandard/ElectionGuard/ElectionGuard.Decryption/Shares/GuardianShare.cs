using ElectionGuard.ElectionSetup;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.Shares;

/// <summary>
/// A challenge for a specific guardian to prove that a decryption share was generated correctly.
/// </summary>
public record GuardianShare : DisposableRecordBase
{
    public string GuardianId { get; init; }

    public ulong SequenceOrder { get; init; }

    public TallyShare Tally { get; init; }

    public List<BallotShare> Ballots { get; init; }

    public GuardianShare(
        IElectionGuardian guardian,
        TallyShare tally,
        List<BallotShare> ballots)
    {
        GuardianId = guardian.GuardianId;
        SequenceOrder = guardian.SequenceOrder;
        Tally = tally;
        Ballots = ballots;
    }


    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Tally.Dispose();
        // TODO Ballots.Dispose();
    }
}
