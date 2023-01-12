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

        public string Id { get; }
        public CeremonyDetails CeremonyDetails { get; }

        // From Guardians
        // Round 1
        private readonly Dictionary<string, ElectionPublicKey> _electionPublicKeys = new();

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
