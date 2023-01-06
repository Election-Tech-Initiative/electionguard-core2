namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// Verification of election partial key used in key sharing
    /// </summary>
    public class ElectionPartialKeyVerification
    {
        public string OwnerId { get; set; }
        public string DesignatedId { get; internal set; }
        public string VerifierId { get; set; }
        public bool Verified { get; internal set; }
    }
}