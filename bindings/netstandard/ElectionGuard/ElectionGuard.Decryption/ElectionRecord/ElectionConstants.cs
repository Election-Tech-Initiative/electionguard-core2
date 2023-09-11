using ElectionGuard.ElectionSetup;

namespace ElectionGuard.Decryption.ElectionRecord;

/// <summary>
/// Serializable election constants used for a specific election
/// </summary>
public record ElectionConstants : DisposableRecordBase
{
    [JsonProperty("generator")]
    public ElementModP G { get; init; }

    [JsonProperty("large_prime")]
    public ElementModP P { get; init; }

    [JsonProperty("small_prime")]
    public ElementModQ Q { get; init; }

    [JsonProperty("cofactor")]
    public ElementModP R { get; init; }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();
        G?.Dispose();
        P?.Dispose();
        Q?.Dispose();
        R?.Dispose();
    }

    public static ElectionConstants Current()
    {
        return new ElectionConstants
        {
            G = Constants.G,
            P = Constants.P,
            Q = Constants.Q,
            R = Constants.R
        };
    }
}
