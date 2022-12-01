using System.Collections.Generic;
using ElGamalPublicKey=ElectionGuard.ElementModP;
using PublicCommitment = ElectionGuard.ElementModP;

namespace ElectionGuard.ElectionSetup
{
    /// <summary>
    /// A tuple of election public key and owner information
    /// </summary>
    public class ElectionPublicKey
    {
        public ElectionPublicKey(
            string ownerId, 
            int sequenceOrder,
            ElGamalPublicKey publicKey, 
            List<PublicCommitment> publicCommitments,
            List<SchnorrProof> coefficientProofs)
        {
            OwnerId = ownerId;
            SequenceOrder = sequenceOrder;
            Key = publicKey;
            CoefficientCommitments = publicCommitments;
            CoefficientProofs = coefficientProofs;
        }

        /// <summary>
        /// The id of the owner guardian
        /// </summary>
        public string OwnerId { get; }

        /// <summary>
        /// The sequence order of the owner guardian
        /// </summary>
        public int SequenceOrder { get; }

        /// <summary>
        /// The election public for the guardian
        /// Note: This is the same as the first coefficient commitment
        /// </summary>
        public ElGamalPublicKey Key { get; }

        /// <summary>
        /// The commitments for the coefficients in the secret polynomial
        /// </summary>
        public List<PublicCommitment> CoefficientCommitments { get; }

        /// <summary>
        /// The proofs for the coefficients in the secret polynomial
        /// </summary>
        public List<SchnorrProof> CoefficientProofs { get; }
    }
}