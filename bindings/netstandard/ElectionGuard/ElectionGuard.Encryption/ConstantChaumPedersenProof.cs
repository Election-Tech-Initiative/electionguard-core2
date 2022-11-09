namespace ElectionGuard
{
    /// <Summary>
    /// The Constant Chaum PedersenProof is a Non-Interactive Zero-Knowledge Proof
    /// that represents the proof of satisfying the selection limit (that the voter has not over voted).
    /// The proof demonstrates that the elgamal accumulation of the encrypted selections
    /// on the ballot forms an aggregate contest encryption matches the combination of random nonces (R)
    /// used to encrypt the selections and that the encrypted values do not exceed the selection limit L.
    ///
    /// This object should not be made directly.  Use ConstantChaumPedersenProof::make
    ///
    /// see: https://www.electionguard.vote/spec/0.95.0/5_Ballot_encryption/#proof-of-satisfying-the-selection-limit
    /// </Summary>
    public class ConstantChaumPedersenProof : DisposableBase
    {
        /// <Summary>
        /// a in the spec
        /// </Summary>
        public unsafe ElementModP Pad
        {
            get
            {
                var status = NativeInterface.ConstantChaumPedersenProof.GetPad(
                    Handle, out NativeInterface.ElementModP.ElementModPHandle value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// b in the spec
        /// </Summary>
        public unsafe ElementModP Data
        {
            get
            {
                var status = NativeInterface.ConstantChaumPedersenProof.GetData(
                    Handle, out NativeInterface.ElementModP.ElementModPHandle value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// c in the spec
        /// </Summary>
        public unsafe ElementModQ Challenge
        {
            get
            {
                var status = NativeInterface.ConstantChaumPedersenProof.GetChallenge(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// v in the spec
        /// </Summary>
        public unsafe ElementModQ Response
        {
            get
            {
                var status = NativeInterface.ConstantChaumPedersenProof.GetResponse(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        internal unsafe NativeInterface.ConstantChaumPedersenProof.ConstantChaumPedersenProofHandle Handle;

        internal unsafe ConstantChaumPedersenProof(NativeInterface.ConstantChaumPedersenProof.ConstantChaumPedersenProofHandle handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// make function for a `ConstantChaumPedersenProof`
        ///
        /// <param name="message"> The ciphertext message</param>
        /// <param name="r"> The nonce used creating the ElGamal ciphertext</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="seed"> A value used when generating the challenge,
        ///          usually the election extended base hash (𝑄')</param>
        /// <param name="hashHeader">Used to generate other random values here</param>
        /// <param name="constant">The constant value to prove</param>
        /// <returns>An instance</returns>
        /// </Summary>
        public unsafe ConstantChaumPedersenProof(
            ElGamalCiphertext message,
            ElementModQ r,
            ElementModP k,
            ElementModQ seed,
            ElementModQ hashHeader,
            ulong constant)
        {
            var status = NativeInterface.ConstantChaumPedersenProof.Make(
                message.Handle, r.Handle, k.Handle, seed.Handle, hashHeader.Handle, constant, out Handle);
            status.ThrowIfError();
        }

        /// <Summary>
        /// Validates a `ConstantChaumPedersenProof`
        ///
        /// <param name="message"> The ciphertext message</param>
        /// <param name="k"> The public key of the election</param>
        /// <param name="q"> The extended base hash of the election</param>
        /// <returns> True if everything is consistent. False otherwise. </returns>
        /// </Summary>
        public unsafe bool IsValid(ElGamalCiphertext message, ElementModP k, ElementModQ q)
        {
            return NativeInterface.ConstantChaumPedersenProof.IsValid(
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