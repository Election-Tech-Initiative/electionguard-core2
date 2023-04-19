namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Election partial key backup used for key sharing
/// </summary>
public class ElectionPartialKeyBackup : DisposableBase
{
    public string OwnerId { get; set; }

    public string DesignatedId { get; set; }

    public ulong DesignatedSequenceOrder { get; set; }

    public HashedElGamalCiphertext EncryptedCoordinate { get; set; }

    public ElectionPartialKeyBackup(
        string ownerId,
        string designatedId,
        ulong designatedSequenceOrder,
        HashedElGamalCiphertext encryptedCoordinate)
    {
        OwnerId = ownerId;
        DesignatedId = designatedId;
        DesignatedSequenceOrder = designatedSequenceOrder;
        EncryptedCoordinate = new(encryptedCoordinate);
    }

    public ElectionPartialKeyBackup(ElectionPartialKeyBackup other)
    {
        OwnerId = other.OwnerId;
        DesignatedId = other.DesignatedId;
        DesignatedSequenceOrder = other.DesignatedSequenceOrder;
        EncryptedCoordinate = new(other.EncryptedCoordinate);
    }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        EncryptedCoordinate?.Dispose();
    }
}
