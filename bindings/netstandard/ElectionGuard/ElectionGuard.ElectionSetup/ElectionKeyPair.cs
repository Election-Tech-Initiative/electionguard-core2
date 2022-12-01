namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// A tuple of election key pair, proof and polynomial
    /// </summary>
    public class ElectionKeyPair
    {
        public ElectionKeyPair(string guardianId, int sequenceOrder, ElGamalKeyPair keyPair, ElectionPolynomial polynomial)
        {
            OwnerId = guardianId;
            SequenceOrder = sequenceOrder;
            KeyPair = keyPair;
            Polynomial = polynomial;
        }

        /// <summary>
        /// The id of the owner guardian
        /// </summary>
        public string OwnerId { get; set; }

        /// <summary>
        /// The sequence order of the owner guardian
        /// </summary>
        public int SequenceOrder { get; set; }

        /// <summary>
        /// The pair of public and private election keys for the guardian
        /// </summary>
        public ElGamalKeyPair KeyPair { get; set; }

        /// <summary>
        /// The secret polynomial for the guardian
        /// </summary>
        public ElectionPolynomial Polynomial { get; set; }

        public ElectionPublicKey Share()
        {
            return new ElectionPublicKey(
                OwnerId,
                SequenceOrder,
                KeyPair.PublicKey,
                Polynomial.GetCommitments(),
                Polynomial.GetProofs()
            );
        }

        public static ElectionKeyPair GenerateElectionKeyPair(string guardianId, int sequenceOrder, int quorum, ElementModQ nonce)
        {
            var polynomial = ElectionPolynomial.GeneratePolynomial(quorum, nonce);
            var privateKey = polynomial.Coefficients[0].Value;
            var publicKey = polynomial.Coefficients[0].Commitment;
            // todo: add the publicKey into the ElGamalKeyPair constructor and set into C++
            var keyPair = new ElGamalKeyPair(privateKey);
            return new ElectionKeyPair(guardianId, sequenceOrder, keyPair, polynomial);
        }
    }
}