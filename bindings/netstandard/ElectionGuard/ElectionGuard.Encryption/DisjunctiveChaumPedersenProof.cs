using System;
namespace ElectionGuard
{
    // Declare native types for convenience
    using NativeElementModP = NativeInterface.ElementModP.ElementModPHandle;
    using NativeElementModQ = NativeInterface.ElementModQ.ElementModQHandle;
    using NativeElGamalCiphertext = NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle;
    using NativeDisjunctiveChaumPedersenProof = NativeInterface.DisjunctiveChaumPedersenProof.DisjunctiveChaumPedersenProofHandle;

    /// <Summary>
    /// The Disjunctive Chaum Pederson proof is a Non-Interactive Zero-Knowledge Proof
    /// that represents the proof of ballot correctness (that a value is either zero or one).
    /// This proof demonstrates that an ElGamal encryption pair (𝛼,𝛽) is an encryption of zero or one
    /// (given knowledge of encryption nonce R).
    ///
    /// This object should not be constructed directly.  Use DisjunctiveChaumPedersenProof::make
    ///
    /// see: https://www.electionguard.vote/spec/0.95.0/5_Ballot_encryption/#outline-for-proofs-of-ballot-correctness
    /// </Summary>
    public class DisjunctiveChaumPedersenProof : DisposableBase
    {
        /// <Summary>
        /// a0 in the spec
        /// </Summary>
        public unsafe ElementModP ZeroPad
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetZeroPad(
                    Handle, out NativeElementModP value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// b0 in the spec
        /// </Summary>
        public unsafe ElementModP ZeroData
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetZeroData(
                    Handle, out NativeElementModP value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// a1 in the spec
        /// </Summary>
        public unsafe ElementModP OnePad
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetOnePad(
                    Handle, out NativeElementModP value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// b1 in the spec
        /// </Summary>
        public unsafe ElementModP OneData
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetOneData(
                    Handle, out NativeElementModP value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// c0 in the spec
        /// </Summary>
        public unsafe ElementModQ ZeroChallenge
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetZeroChallenge(
                    Handle, out NativeElementModQ value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// c1 in the spec
        /// </Summary>
        public unsafe ElementModQ OneChallenge
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetOneChallenge(
                    Handle, out NativeElementModQ value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// c in the spec
        /// </Summary>
        public unsafe ElementModQ Challenge
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetChallenge(
                    Handle, out NativeElementModQ value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// v0 in the spec
        /// </Summary>
        public unsafe ElementModQ ZeroResponse
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetZeroResponse(
                    Handle, out NativeElementModQ value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// v1 in the spec
        /// </Summary>
        public unsafe ElementModQ OneResponse
        {
            get
            {
                var status = NativeInterface.DisjunctiveChaumPedersenProof.GetOneResponse(
                    Handle, out NativeElementModQ value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        internal unsafe NativeDisjunctiveChaumPedersenProof Handle;

        unsafe internal DisjunctiveChaumPedersenProof(NativeDisjunctiveChaumPedersenProof handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// make function for a `DisjunctiveChaumPedersenProof`
        ///
        /// This overload does not accept a seed value and calculates
        /// proofs independent of the original encryption. (faster performance)
        /// <param name="message"> The ciphertext message</param>
        /// <param name="r"> The nonce used creating the ElGamal ciphertext</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> A value used when generating the challenge,
        ///          usually the election extended base hash (𝑄')</param>
        /// <param name="plaintext">The constant value to prove, zero or one</param>
        /// <returns>A unique pointer</returns>
        /// </Summary>
        unsafe public DisjunctiveChaumPedersenProof(
            ElGamalCiphertext message,
            ElementModQ r,
            ElementModP k,
            ElementModQ q,
            ulong plaintext)
        {
            var status = NativeInterface.DisjunctiveChaumPedersenProof.Make(
                message.Handle, r.Handle, k.Handle, q.Handle, plaintext, out Handle);
            status.ThrowIfError();
        }

        /// <Summary>
        /// make function for a `DisjunctiveChaumPedersenProof`
        ///
        /// This overload accepts a seed value and calculates
        /// proofs deterministically based on the seed. (slower, but reproduceable proofs)
        /// <param name="message"> The ciphertext message</param>
        /// <param name="r"> The nonce used creating the ElGamal ciphertext</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> A value used when generating the challenge,
        ///          usually the election extended base hash (𝑄')</param>
        /// <param name="seed">Used to generate other random values here</param>
        /// <param name="plaintext">The constant value to prove, zero or one</param>
        /// <returns>A unique pointer</returns>
        /// </Summary>
        unsafe public DisjunctiveChaumPedersenProof(
            ElGamalCiphertext message,
            ElementModQ r,
            ElementModP k,
            ElementModQ q,
            ElementModQ seed,
            ulong plaintext)
        {
            var status = NativeInterface.DisjunctiveChaumPedersenProof.Make(
                message.Handle, r.Handle, k.Handle, q.Handle, seed.Handle, plaintext, out Handle);
            status.ThrowIfError();
        }

        /// <Summary>
        /// Validates a "disjunctive" Chaum-Pedersen (zero or one) proof.
        ///
        /// <param name="message"> The ciphertext message</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> The extended base hash of the election</param>
        /// <returns> True if everything is consistent. False otherwise. </returns>
        /// </Summary>
        public unsafe bool IsValid(ElGamalCiphertext message, ElementModP k, ElementModQ q)
        {
            return NativeInterface.DisjunctiveChaumPedersenProof.IsValid(
                Handle, message.Handle, k.Handle, q.Handle
            );
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override unsafe void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }
    }
}
