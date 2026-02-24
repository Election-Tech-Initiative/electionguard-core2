namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// Election partial key backup used for key sharing
    /// </summary>
    public class ElectionPartialKeyBackupAttribute : DisposableBase
    {
        /// <summary>
        /// The Id of the guardian that generated this backup
        /// </summary>
        public string? OwnerId { get; init; }

        /// <summary>
        /// The Id of the guardian to receive this backup
        /// </summary>
        public string? DesignatedId { get; init; }

        /// <summary>
        /// The sequence order of the designated guardian
        /// </summary>
        public int DesignatedSequenceOrder { get; init; }

        /// <summary>
        /// The coordinate corresponding to a secret election polynomial
        /// </summary>
        public HashedElGamalCiphertext? EncryptedCoordinate { get; init; }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();
            EncryptedCoordinate?.Dispose();
        }
    }
}