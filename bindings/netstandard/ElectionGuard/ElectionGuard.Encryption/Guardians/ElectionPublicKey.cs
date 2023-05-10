using ElectionGuard.Proofs;
using ElectionGuard.Extensions;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace ElectionGuard.Guardians
{

    /// <summary>
    /// A tuple of election public key and owner information
    /// </summary>
    public class ElectionPublicKey : DisposableBase, IElectionGuardian
    {
        [JsonConstructor]
        public ElectionPublicKey(
            string guardianId,
            ulong sequenceOrder,
            ElementModP key,
            List<ElementModP> coefficientCommitments,
            List<SchnorrProof> coefficientProofs)
        {
            GuardianId = guardianId;
            SequenceOrder = sequenceOrder;
            Key = new ElementModP(key);
            CoefficientCommitments = coefficientCommitments
                .Select(x => new ElementModP(x)).ToList();
            CoefficientProofs = coefficientProofs
                .Select(x => new SchnorrProof(x)).ToList();
        }

        public ElectionPublicKey(ElectionPublicKey other)
        {
            GuardianId = other.GuardianId;
            SequenceOrder = other.SequenceOrder;
            Key = new ElementModP(other.Key);
            CoefficientCommitments = other.CoefficientCommitments
                .Select(x => new ElementModP(x)).ToList();
            CoefficientProofs = other.CoefficientProofs
                .Select(x => new SchnorrProof(x)).ToList();
        }

        public string ObjectId => GuardianId;

        public string GuardianId { get; set; }

        /// <summary>
        /// The sequence order of the owner guardian
        /// </summary>
        public ulong SequenceOrder { get; set; }

        /// <summary>
        /// The election public for the guardian
        /// Note: This is the same as the first coefficient commitment
        /// </summary>
        public ElementModP Key { get; set; }

        /// <summary>
        /// The commitments for the coefficients in the secret polynomial
        ///
        /// 𝐾𝑖,j = g^𝑎𝑖,j mod p in the spec Equation (7) 
        /// </summary>
        public List<ElementModP> CoefficientCommitments { get; set; }

        /// <summary>
        /// The proofs for the coefficients in the secret polynomial
        /// </summary>
        public List<SchnorrProof> CoefficientProofs { get; set; }

        protected override void DisposeUnmanaged()
        {
            base.DisposeUnmanaged();

            Key.Dispose();
            CoefficientCommitments.Dispose();
            CoefficientProofs.Dispose();
        }
    }
}
