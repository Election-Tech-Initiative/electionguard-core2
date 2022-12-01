using SecretCoefficient=ElectionGuard.ElementModQ;
using PublicCommitment=ElectionGuard.ElementModP;

namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// A coefficient of an Election Polynomial
    /// </summary>
    public class Coefficient
    {
        /// <summary>
        /// The secret coefficient `a_ij` 
        /// </summary>
        public SecretCoefficient Value { get; set; } 

        /// <summary>
        /// The public key `K_ij` generated from secret coefficient
        /// </summary>
        public PublicCommitment Commitment { get; set; }

        /// <summary>
        /// A proof of possession of the private key for the secret coefficient
        /// </summary>
        public SchnorrProof Proof { get; set; }
    }
}