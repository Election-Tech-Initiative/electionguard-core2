using System;
using NativeElGamalCiphertext = ElectionGuard.NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle;

namespace ElectionGuard
{
    /// <summary>
    /// An "exponential ElGamal ciphertext" (i.e., with the plaintext in the exponent to allow for
    /// homomorphic addition). Create one with `elgamal_encrypt`. Add them with `elgamal_add`.
    /// Decrypt using one of the supplied instance methods.
    /// </summary>
    public class ElGamalCiphertext : DisposableBase, IEquatable<ElGamalCiphertext>
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
                return value.IsInvalid ? null : new ElementModP(value);
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
                return value.IsInvalid ? null : new ElementModP(value);
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
                return value.IsInvalid ? null : new ElementModQ(value);
            }
        }

        /// <summary>
        /// Determines if the element is valid and has not been cleaned up
        /// </summary>
        public bool IsAddressable => Handle != null && !Handle.IsInvalid && !IsDisposed;

        internal NativeElGamalCiphertext Handle;

        public ElGamalCiphertext(ElementModP pad, ElementModP data)
        {
            var status = NativeInterface.ElGamalCiphertext.New(
                pad.Handle, data.Handle, out var handle);
            status.ThrowIfError();
            handle.ThrowIfInvalid();
            Handle = handle;
        }

        public ElGamalCiphertext(ElGamalCiphertext other)
        {
            var status = NativeInterface.ElGamalCiphertext.New(
                other.Pad.Handle, other.Data.Handle, out var handle);
            status.ThrowIfError();
            handle.ThrowIfInvalid();
            Handle = handle;
        }

        internal ElGamalCiphertext(NativeElGamalCiphertext handle)
        {
            handle.ThrowIfInvalid();
            Handle = handle;
        }

        /// <summary>
        /// Homomorphically accumulates other ElGamal ciphertext by pairwise multiplication
        /// and returns the result without modifying the original.
        /// </summary>
        public ElGamalCiphertext Add(ElGamalCiphertext other)
        {
            var status = NativeInterface.ElGamalCiphertext.Add(
                Handle, other.Handle, out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElGamalCiphertext(value);
            //return ElGamal.Add(this, other);
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
            return value.IsInvalid ? null : new ElementModP(value);
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

        public override string ToString()
        {
            return $"ElGamalCiphertext(Pad: {Pad}, Data: {Data}, CryptoHash: {CryptoHash})";
        }

        # region IEquatable

        /// <inheritdoc />
        public static bool operator ==(ElGamalCiphertext a, ElGamalCiphertext b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.Pad == b.Pad && a.Data == b.Data;
        }

        public static bool operator !=(ElGamalCiphertext a, ElGamalCiphertext b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Check to see if the object is equal to the current instance 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ElGamalCiphertext);
        }

        public bool Equals(ElGamalCiphertext other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return Pad == other.Pad && Data == other.Data;
        }

        /// <summary>
        /// Generates a hashcode for the class
        /// </summary>
        /// <returns>the hashcode</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ToString());
            return hashCode.GetHashCode();
        }
        #endregion
    }
}
