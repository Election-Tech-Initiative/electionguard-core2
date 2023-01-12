namespace ElectionGuard.ElectionSetup;

/// <summary>
/// Election partial key backup used for key sharing
/// </summary>
public record ElectionPartialKeyBackup
{
    public string? OwnerId { get; init; } = default;

    public string? DesignedId { get; init; } = default;

    public ulong DesignatedSequenceOrder { get; init; } = default;

    public HashedElGamalCiphertext? EncryptedCoordinate { get; init; } = default;
}
