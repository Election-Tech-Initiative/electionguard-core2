namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Election partial key backup used for key sharing
/// </summary>
public class ElectionPartialKeyBackup : DisposableBase
{
    public string? OwnerId { get; init; } = default;

    public string? DesignatedId { get; init; } = default;

    public ulong DesignatedSequenceOrder { get; init; } = default;

    public HashedElGamalCiphertext? EncryptedCoordinate { get; init; } = default;

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        EncryptedCoordinate?.Dispose();
    }
}
