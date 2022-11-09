namespace ElectionGuard
{
    /// <summary>
    /// An "exponential ElGamal ciphertext" (i.e., with the plaintext in the exponent to allow for
    /// homomorphic addition). Create one with `elgamal_encrypt`. Add them with `elgamal_add`.
    /// Decrypt using one of the supplied instance methods.
    /// </summary>
    public class ElGamalCiphertext : DisposableBase
    {
        /// <Summary>
        /// The pad value also referred to as A, a, 𝑎, or alpha in the spec.
        /// </Summary>
        public unsafe ElementModP Pad
        {
            get
            {
                var status = NativeInterface.ElGamalCiphertext.GetPad(
                    Handle, out NativeInterface.ElementModP.ElementModPHandle value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// The data value also referred to as B, b, 𝛽, or beta in the spec.
        /// </Summary>
        public unsafe ElementModP Data
        {
            get
            {
                var status = NativeInterface.ElGamalCiphertext.GetData(
                    Handle, out NativeInterface.ElementModP.ElementModPHandle value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <summary>
        /// Get the CryptoHash
        /// </summary>
        public unsafe ElementModQ CryptoHash
        {
            get
            {
                var status = NativeInterface.ElGamalCiphertext.GetCryptoHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        internal unsafe NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle Handle;

        internal unsafe ElGamalCiphertext(NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// Decrypt the ciphertext directly using the provided secret key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public unsafe ulong? Decrypt(ElementModQ secretKey)
        {
            ulong plaintext = 0;
            var status = NativeInterface.ElGamalCiphertext.DecryptWithSecret(
                Handle, secretKey.Handle, ref plaintext);
            status.ThrowIfError();
            return plaintext;
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