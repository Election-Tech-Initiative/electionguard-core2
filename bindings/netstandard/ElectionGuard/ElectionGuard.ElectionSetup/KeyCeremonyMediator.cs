using MediatorId=System.String;
using GuardianId=System.String;
using System.Collections.Generic;

namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// KeyCeremonyMediator for assisting communication between guardians
    /// </summary>
    public class KeyCeremonyMediator
    {
        public KeyCeremonyMediator(string mediatorId, CeremonyDetails ceremonyDetails)
        {
            Id = mediatorId;
            CeremonyDetails = ceremonyDetails;
        }

        public MediatorId Id { get; }
        public CeremonyDetails CeremonyDetails { get; }

        // From Guardians
        // Round 1
        private readonly Dictionary<GuardianId, ElectionPublicKey> _electionPublicKeys = new Dictionary<string, ElectionPublicKey>();

        public void Announce(ElectionPublicKey shareKey)
        {
            ReceiveElectionPublicKey(shareKey);
        }

        /// <summary>
        /// Receive election public key from guardian
        /// </summary>
        /// <param name="publicKey">Election public key</param>
        private void ReceiveElectionPublicKey(ElectionPublicKey publicKey)
        {
            _electionPublicKeys[publicKey.OwnerId] = publicKey;
        }
    }
}
