using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.ElectionRecord;

/// <summary>
/// Serializable election constants used for a specific election
/// </summary>
public record ElectionConstants : DisposableRecordBase
{
    public ElementModP G { get; init; }
    public ElementModP P { get; init; }
    public ElementModQ Q { get; init; }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        G?.Dispose();
        P?.Dispose();
        Q?.Dispose();
    }

    public static ElectionConstants Current()
    {
        return new ElectionConstants
        {
            G = Constants.G,
            P = Constants.P,
            Q = Constants.Q
        };
    }
}
