using System.Collections.Generic;
using NativeElGamalCiphertext = ElectionGuard.NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle;

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
        public ElementModP Pad
        {
            get
            {
                var status = NativeInterface.ElGamalCiphertext.GetPad(
                    Handle, out var value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// The data value also referred to as B, b, 𝛽, or beta in the spec.
        /// </Summary>
        public ElementModP Data
        {
            get
            {
                var status = NativeInterface.ElGamalCiphertext.GetData(
                    Handle, out var value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModP(value);
            }
        }

        /// <summary>
        /// Get the CryptoHash
        /// </summary>
        public ElementModQ CryptoHash
        {
            get
            {
                var status = NativeInterface.ElGamalCiphertext.GetCryptoHash(
                    Handle, out var value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModQ(value);
            }
        }

        internal NativeElGamalCiphertext Handle;

        public ElGamalCiphertext(ElementModP pad, ElementModP data)
        {
            var status = NativeInterface.ElGamalCiphertext.New(
                pad.Handle, data.Handle, out var handle);
            status.ThrowIfError();
            Handle = handle;
        }

        internal ElGamalCiphertext(NativeElGamalCiphertext handle)
        {
            Handle = handle;
        }

        public ElGamalCiphertext Add(ElGamalCiphertext other)
        {
            return ElGamal.Add(new List<ElGamalCiphertext> { this, other });
        }

        /// <Summary>
        /// Decrypts an ElGamal ciphertext with a "known product" (the blinding factor used in the encryption).
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public ulong? Decrypt(ElementModP knownProduct)
        {
            ulong plaintext = 0;
            var status = NativeInterface.ElGamalCiphertext.DecryptKnownProduct(
                Handle, knownProduct.Handle, ref plaintext);
            status.ThrowIfError();
            return plaintext;
        }

        /// <Summary>
        /// Decrypt the ciphertext directly using the provided secret key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public ulong? Decrypt(ElementModQ secretKey)
        {
            ulong plaintext = 0;
            var status = NativeInterface.ElGamalCiphertext.DecryptWithSecret(
                Handle, secretKey.Handle, ref plaintext);
            status.ThrowIfError();
            return plaintext;
        }

        /// <Summary>
        /// Decrypt an ElGamal ciphertext using a known nonce and the ElGamal public key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public ulong? Decrypt(ElementModP publicKey, ElementModQ nonce)
        {
            ulong plaintext = 0;
            var status = NativeInterface.ElGamalCiphertext.DecryptKnownNonce(
                Handle, publicKey.Handle, nonce.Handle, ref plaintext);
            status.ThrowIfError();
            return plaintext;
        }

        /// <Summary>
        /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
        ///
        /// 𝑀_i = 𝐴^𝑠𝑖 mod 𝑝 in the spec
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public ElementModP PartialDecrypt(ElementModQ secretKey)
        {
            var status = NativeInterface.ElGamalCiphertext.PartialDecrypt(
                Handle, secretKey.Handle, out var value);
            status.ThrowIfError();
            return new ElementModP(value);
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid)
            {
                return;
            }

            Handle.Dispose();
            Handle = null;
        }
    }
}
