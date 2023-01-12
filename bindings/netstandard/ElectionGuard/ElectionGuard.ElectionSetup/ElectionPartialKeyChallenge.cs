namespace ElectionGuard.ElectionSetup
{
    public class ElectionPartialKeyChallenge
    {
        public string? OwnerId { get; init; }
        public string? DesignatedId { get; init; }
        public ulong DesignatedSequenceOrder { get; init; }
        public ElementModQ? Value { get; init; }
        public List<ElementModP>? CoefficientCommitments { get; init; }
        public List<SchnorrProof>? CoefficientProofs { get; init; }
    }
}
