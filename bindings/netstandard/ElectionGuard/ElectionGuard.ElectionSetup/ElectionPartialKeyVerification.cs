namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// Verification of election partial key used in key sharing
    /// </summary>
    public record ElectionPartialKeyVerification
    {
        public string? OwnerId { get; init; }
        public string? DesignatedId { get; init; }
        public string? VerifierId { get; init; }
        public bool Verified { get; init; }
    }
}