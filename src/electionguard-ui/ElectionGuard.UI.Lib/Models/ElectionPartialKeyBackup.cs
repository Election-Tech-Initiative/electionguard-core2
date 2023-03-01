namespace ElectionGuard.UI.Lib.Models;

/// <summary>
/// Election partial key backup used for key sharing
/// </summary>
public class ElectionPartialKeyBackup : DisposableBase
{
    public string? OwnerId { get; set; } = default;

    public string? DesignatedId { get; set; } = default;

    public ulong DesignatedSequenceOrder { get; set; } = default;

    public HashedElGamalCiphertext? EncryptedCoordinate { get; set; } = default;

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        EncryptedCoordinate?.Dispose();
    }
}
