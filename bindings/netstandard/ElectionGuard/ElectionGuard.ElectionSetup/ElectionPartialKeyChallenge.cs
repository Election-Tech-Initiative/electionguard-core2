using ElectionGuard.ElectionSetup.Extensions;
using ElectionGuard.Proofs;
using ElectionGuard.Extensions;

namespace ElectionGuard.ElectionSetup;
public class ElectionPartialKeyChallenge : DisposableBase
{
    public string? OwnerId { get; init; }
    public string? DesignatedId { get; init; }
    public ulong DesignatedSequenceOrder { get; init; }
    public ElementModQ? Value { get; init; }
    public List<ElementModP>? CoefficientCommitments { get; init; }
    public List<SchnorrProof>? CoefficientProofs { get; init; }

    protected override void DisposeUnmanaged()
    {
        base.DisposeUnmanaged();

        Value?.Dispose();

        CoefficientCommitments?.Dispose();
        CoefficientProofs?.Dispose();
    }
}
