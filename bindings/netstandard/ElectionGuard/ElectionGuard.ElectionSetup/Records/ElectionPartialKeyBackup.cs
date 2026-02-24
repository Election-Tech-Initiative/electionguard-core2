namespace ElectionGuard.ElectionSetup.Records;

/// <summary>
/// Election partial key backup used for key sharing
/// </summary>
public record ElectionPartialKeyBackupRecord : DisposableRecordBase
{
    // TODO: implement IElectionGuardian

    /// <summary>
    /// The guardian id of the owner who created this backup
    /// </summary>
    public string OwnerId { get; set; }

    public ulong OwnerSequenceOrder { get; set; }

    /// <summary>
    /// The guardian id of the designated guardian who should receive this backup
    /// </summary>
    public string DesignatedId { get; set; }

    public ulong DesignatedSequenceOrder { get; set; }

    public HashedElGamalCiphertext EncryptedCoordinate { get; set; }

    public ElectionPartialKeyBackupRecord(
        string ownerId,
        ulong ownerSequenceOrder,
        string designatedId,
        ulong designatedSequenceOrder,
        HashedElGamalCiphertext encryptedCoordinate)
    {
        OwnerId = ownerId;
        OwnerSequenceOrder = ownerSequenceOrder;
        DesignatedId = designatedId;
        DesignatedSequenceOrder = designatedSequenceOrder;
        EncryptedCoordinate = new(encryptedCoordinate);
    }


    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        EncryptedCoordinate?.Dispose();
    }
}
