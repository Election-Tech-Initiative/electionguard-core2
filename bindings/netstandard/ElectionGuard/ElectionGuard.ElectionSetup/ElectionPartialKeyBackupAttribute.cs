using System;

namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// Election partial key backup used for key sharing
    /// </summary>
    public class ElectionPartialKeyBackupAttribute
    {
        /// <summary>
        /// The Id of the guardian that generated this backup
        /// </summary>
        private string OwnerId { get; }

        /// <summary>
        /// The Id of the guardian to receive this backup
        /// </summary>
        private string DesignatedId { get; }

        /// <summary>
        /// The sequence order of the designated guardian
        /// </summary>
        private int DesignatedSequenceOrder { get; }

        /// <summary>
        /// The coordinate corresponding to a secret election polynomial
        /// </summary>
        private HashedElGamalCiphertext EncryptedCoordinate { get; }
    }
}