namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// Representation of a Schnorr proof
    /// </summary>
    public class SchnorrProof
    {
        /// <summary>
        /// k in the spec
        /// </summary>
        public ElGamalPublicKey PublicKey { get; set; }

        /// <summary>
        /// h in the spec
        /// </summary>
        public ElementModP Commitment { get; set; }

        /// <summary>
        /// c in the spec
        /// </summary>
        public ElementModQ Challenge { get; set; }

        /// <summary>
        /// u in the spec
        /// </summary>
        public ElementModQ Response { get; set; }

        public ProofUsage Usage = ProofUsage.SecretValue;
    }
}