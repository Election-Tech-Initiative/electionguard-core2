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
        /// Reassign this object's handle by taking ownership of the handle from the other object
        /// but does not explicitly dispose the other object in order to maintain compatibility
        /// with the `using` directive. This method is similar to `std::move` in C++ since we cannot
        /// override the assignment operator in csharp.
        ///
        /// This is useful for avoiding unnecessary copies of large objects when passing them
        /// and assigning them to new variables. It is also useful for avoiding unnecessary
        /// allocations when reassigning objects in a loop.
        /// </summary>
        internal void Reassign(ElGamalCiphertext other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            Reassign(other.Handle);
            other.Handle = null;
        }

        // TODO: ISSUE #189 - this is a temporary function to handle object reassignment and disposal
        // this should be removed when the native library is updated to handle this behavior
        private void Reassign(NativeElGamalCiphertext other)
        {
            if (other is null)
            {
                return;
            }

            var old = Handle; // assign the old handle to dispose
            Handle = other; // assign the new handle to the instance member
            old.Dispose(); // dispose of the old handle
        }

        /// <summary>
        /// Homomorphically accumulates other ElGamal ciphertext by pairwise multiplication
        /// and returns the result. By default, this method will reassign the current object.
        /// </summary>
        public ElGamalCiphertext Add(ElGamalCiphertext other, bool reassign = true)
        {
            if (!reassign)
            {
                return Add(this, other);
            }

            var newValue = Add(this, other);
            Reassign(newValue);
            return this;
        }

        /// <summary>
        /// Homomorphically accumulates other ElGamal ciphertext by pairwise multiplication
        /// and returns the result without modifying the original.
        /// </summary>
        public static ElGamalCiphertext Add(
            ElGamalCiphertext lhs, ElGamalCiphertext rhs)
        {
            var status = NativeInterface.ElGamalCiphertext.Add(
                lhs.Handle, rhs.Handle, out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElGamalCiphertext(value);
        }

        /// <Summary>
        /// Decrypts an ElGamal ciphertext with an "accumulation" (the product of partial decryptions)
        ///
        /// This is the preferred method for decrypting and is the only method
        /// when decrypting with shares
        /// </Summary>
        public ulong? Decrypt(ElementModP shareAccumulation, ElementModP encryptionBase)
        {
            ulong plaintext = 0;
            var status = NativeInterface.ElGamalCiphertext.DecryptAccumulation(
                Handle, shareAccumulation.Handle, encryptionBase.Handle, ref plaintext);
            status.ThrowIfError();
            return plaintext;
        }

        /// <Summary>
        /// Decrypt the ciphertext directly using the provided secret key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public ulong? Decrypt(ElementModQ secretKey, ElementModP encryptionBase)
        {
            ulong plaintext = 0;
            var status = NativeInterface.ElGamalCiphertext.DecryptWithSecret(
                Handle, secretKey.Handle, encryptionBase.Handle, ref plaintext);
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

            return Pad.Equals(other.Pad) && Data.Equals(other.Data);
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
