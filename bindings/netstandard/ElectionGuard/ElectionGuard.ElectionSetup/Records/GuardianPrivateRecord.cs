namespace ElectionGuard.ElectionSetup.Records;

/// <summary>
/// A guardian is a person or entity that is responsible for decrypting a share of the election record.
/// The GuardianPrivateRecord is a record of the private information that a guardian needs to decrypt.
/// This record contains sensitive information and should be protected.
/// </summary>
public record GuardianPrivateRecord : DisposableRecordBase
{
    public string GuardianId { get; init; }

    public ElectionKeyPair ElectionKeys { get; init; }

    public ElementModQ CommitmentSeed { get; init; }

    public GuardianPrivateRecord(
        string guardianId,
        ElectionKeyPair electionKeys,
        ElementModQ commitmentSeed)
    {
        GuardianId = guardianId;
        ElectionKeys = new(electionKeys);
        CommitmentSeed = new(commitmentSeed);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        ElectionKeys.Dispose();
    }
}
