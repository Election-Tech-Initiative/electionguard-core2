using System;
using ElectionGuard.Decryption.Tally;
using ElectionGuard.ElectionSetup;
using ElectionGuard.Extensions;
using ElectionGuard.Guardians;

namespace ElectionGuard.Decryption.ElectionRecord;

public record ElectionRecordData : DisposableRecordBase
{
    public ElectionConstants Constants { get; init; }
    public List<ElectionPublicKey> Guardians { get; init; }
    public Manifest Manifest { get; init; }
    public CiphertextElectionContext Context { get; init; }
    public List<EncryptionDevice> Devices { get; init; }
    public List<CiphertextBallot> EncryptedBallots { get; init; }
    public List<PlaintextTallyBallot> ChallengedBallots { get; init; }
    public CiphertextTallyRecord EncryptedTally { get; init; }
    public PlaintextTally Tally { get; init; }

    protected override void DisposeManaged()
    {
        base.DisposeManaged();
        Constants?.Dispose();
        Guardians?.Dispose();
        EncryptedTally?.Dispose();
        Tally?.Dispose();
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        Manifest?.Dispose();
        Context?.Dispose();
        Devices?.Dispose();
        EncryptedBallots?.Dispose();
        ChallengedBallots?.Dispose();
    }
}
