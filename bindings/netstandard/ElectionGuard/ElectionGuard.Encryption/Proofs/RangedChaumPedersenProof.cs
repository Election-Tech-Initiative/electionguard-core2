namespace ElectionGuard
{
    using System.Collections.Generic;
    using ElectionGuard.Ballot;
    using NativeElementModP = NativeInterface.ElementModP.ElementModPHandle;
    using NativeElementModQ = NativeInterface.ElementModQ.ElementModQHandle;
    // native types for convenience
    using NativeRangedChaumPedersenProof = NativeInterface.RangedChaumPedersenProof.RangedChaumPedersenProofHandle;

    /// <summary>
    /// The ranged Chaum Pedersen proof is a Non-Interactive Zero-Knowledge Proof that is a generalized version of the 
    /// Disjunctive Chaum Pedersen proof that allows for a variable number of selections to be proven.
    /// </summary>
    public class RangedChaumPedersenProof : DisposableBase
    {
        /// <summary>
        /// Limit on the range proof
        /// </summary>
        public ulong RangeLimit
        {
            get
            {
                var status = NativeInterface.RangedChaumPedersenProof.GetRangeLimit(
                    Handle, out var value);
                status.ThrowIfError();

                return value;
            }
        }

        /// <summary>
        /// The proof's challenge value
        /// </summary>
        public ElementModQ Challenge
        {
            get
            {
                var status = NativeInterface.RangedChaumPedersenProof.GetChallenge(
                    Handle, out var value);
                status.ThrowIfError();

                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// The individual proofs that make up the ranged proof
        /// </summary>
        public Dictionary<ulong, ZeroKnowledgeProof> IntegerProofs => throw new System.NotImplementedException();

        internal NativeRangedChaumPedersenProof Handle;

        internal RangedChaumPedersenProof(NativeRangedChaumPedersenProof handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a new instance of <see cref="RangedChaumPedersenProof"/>. This uses a deterministic seed. 
        /// This ctor is used for unit testing and is not recommended to be directly called in a production election
        /// </summary>
        public RangedChaumPedersenProof(
            ElGamalCiphertext message,
            ElementModQ r,
            ulong selected,
            ulong maxLimit,
            ElementModP k,
            ElementModQ q,
            string hashPrefix,
            ElementModQ seed
        )
        {
            var status = NativeInterface.RangedChaumPedersenProof.Make(
                message.Handle,
                r.Handle,
                selected,
                maxLimit,
                k.Handle,
                q.Handle,
                hashPrefix,
                seed.Handle,
                out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a new instance of <see cref="RangedChaumPedersenProof"/>. This uses a random seed. This ctor is used for production.
        /// </summary>
        public RangedChaumPedersenProof(
            ElGamalCiphertext message,
            ElementModQ r,
            ulong selected,
            ulong maxLimit,
            ElementModP k,
            ElementModQ q,
            string hashPrefix
        )
        {
            var status = NativeInterface.RangedChaumPedersenProof.Make(
                message.Handle,
                r.Handle,
                selected,
                maxLimit,
                k.Handle,
                q.Handle,
                hashPrefix,
                out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Validates the proof against the specified values
        /// </summary>
        public BallotValidationResult IsValid(
            ElGamalCiphertext ciphertext,
            ElementModP publicKey,
            ElementModQ q,
            string hashPrefix)
        {
            var status = NativeInterface.RangedChaumPedersenProof.IsValid(
                Handle, ciphertext.Handle, publicKey.Handle, q.Handle, hashPrefix, out var isValid);
            status.ThrowIfError();

            var message = string.Empty;
            if (!isValid)
            {
                ExceptionHandler.GetData(out var _, out message, out var _);
            }

            return new BallotValidationResult(isValid, message);
        }
    }
}
