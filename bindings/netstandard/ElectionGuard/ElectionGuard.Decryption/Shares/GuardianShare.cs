using ElectionGuard.ElectionSetup;
using ElectionGuard.Extensions;
using ElectionGuard.Guardians;
using Newtonsoft.Json;

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

    [JsonConstructor]
    public GuardianShare(
        string guardianId,
        ulong sequenceOrder,
        TallyShare tally,
        List<BallotShare> ballots)
    {
        GuardianId = guardianId;
        SequenceOrder = sequenceOrder;
        Tally = tally;
        Ballots = ballots;
    }

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


    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Tally.Dispose();
        Ballots.Dispose();
    }
}
