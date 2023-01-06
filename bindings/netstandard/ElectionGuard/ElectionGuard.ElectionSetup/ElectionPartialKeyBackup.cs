namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Election partial key backup used for key sharing
/// </summary>
public record ElectionPartialKeyBackup
{
    public string OwnerId { get; set; }

    public string DesignedId { get; set; }

    public ulong DesignatedSequenceOrder { get; set; }

    public HashedElGamalCiphertext EncryptedCoordinate { get; set; }
}
