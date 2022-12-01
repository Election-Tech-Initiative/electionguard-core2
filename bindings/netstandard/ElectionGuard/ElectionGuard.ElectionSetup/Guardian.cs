using System.Collections.Generic;
using GuardianId=System.String;

namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// Guardian of election responsible for safeguarding information and decrypting results.
    ///
    /// The first half of the guardian involves the key exchange known as the key ceremony.
    /// The second half relates to the decryption process.
    /// </summary>
    public class Guardian
    {
        private readonly ElectionKeyPair _electionKeys;
        private readonly CeremonyDetails _ceremonyDetails;

        /// <summary>
        /// Initialize a guardian with the specified arguments.
        /// </summary>
        /// <param name="keyPair">The key pair the guardian generated during a key ceremony</param>
        /// <param name="ceremonyDetails">The details of the key ceremony</param>
        /// <param name="electionPublicKeys">The public keys the guardian generated during a key ceremony</param>
        /// <param name="partialKeyBackups">The partial key backups the guardian generated during a key ceremony</param>
        /// <param name="backupsToShare"></param>
        /// <param name="guardianElectionPartialKeyVerifications"></param>
        public Guardian(
            ElectionKeyPair keyPair,
            CeremonyDetails ceremonyDetails,
            Dictionary<GuardianId, ElectionPublicKey> electionPublicKeys = null,
            Dictionary<GuardianId, ElectionPartialKeyBackup> partialKeyBackups = null,
            Dictionary<GuardianId, ElectionPartialKeyBackup> backupsToShare = null,
            Dictionary<GuardianId, ElectionPartialKeyVerification>  guardianElectionPartialKeyVerifications = null
            )
        {
            _electionKeys = keyPair;
            _ceremonyDetails = ceremonyDetails;

            // todo: port the rest of the Guardian ctor here
        }

        public static Guardian FromNonce(
            string guardianId,
            int sequenceOrder,
            int numberOfGuardians,
            int quorum,
            ElementModQ nonce = null)
        {
            var keyPair = ElectionKeyPair.GenerateElectionKeyPair(guardianId, sequenceOrder, quorum, nonce);
            var ceremonyDetails = new CeremonyDetails(numberOfGuardians, quorum);
            return new Guardian(keyPair, ceremonyDetails);
        }

        public CeremonyDetails CeremonyDetails { get; set; }

        public ElectionPublicKey ShareKey()
        {
            return _electionKeys.Share();
        }
    }
}
