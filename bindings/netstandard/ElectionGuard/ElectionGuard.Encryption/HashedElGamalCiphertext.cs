using System;
using System.Drawing;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// An "exponential ElGamal ciphertext" (i.e., with the plaintext in the exponent to allow for
    /// homomorphic addition). Create one with `elgamal_encrypt`. Add them with `elgamal_add`.
    /// Decrypt using one of the supplied instance methods.
    /// </summary>
    public class HashedElGamalCiphertext : DisposableBase
    {
        /// <Summary>
        /// The pad value also referred to as A, a, 𝑎, or alpha in the spec.
        /// </Summary>
        public ElementModP Pad
        {
            get
            {
                var status = NativeInterface.HashedElGamalCiphertext.GetPad(
                    Handle, out NativeInterface.ElementModP.ElementModPHandle value);
                status.ThrowIfError();
                return new ElementModP(value);
            }
        }

        /// <Summary>
        /// The data value also referred to as B, b, 𝛽, or beta in the spec.
        /// </Summary>
        public byte[] Data
        {
            get
            {
                var status = NativeInterface.HashedElGamalCiphertext.GetData(
                    Handle, out IntPtr data, out ulong size);
                status.ThrowIfError();
                var byteArray = new byte[(int)size];
                Marshal.Copy(data, byteArray, 0, (int)size);
                NativeInterface.Memory.DeleteIntPtr(data);
                return byteArray;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <value></value>
        public byte[] Mac
        {
            get
            {
                var status = NativeInterface.HashedElGamalCiphertext.GetMac(
                    Handle, out IntPtr data, out ulong size);
                status.ThrowIfError();
                var byteArray = new byte[(int)size];
                Marshal.Copy(data, byteArray, 0, (int)size);
                NativeInterface.Memory.DeleteIntPtr(data);
                return byteArray;
            }
        }

        /// <summary>
        /// Get the CryptoHash
        /// </summary>
        public ElementModQ CryptoHash
        {
            get
            {
                var status = NativeInterface.HashedElGamalCiphertext.GetCryptoHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        internal NativeInterface.HashedElGamalCiphertext.HashedElGamalCiphertextHandle Handle;

        internal HashedElGamalCiphertext(NativeInterface.HashedElGamalCiphertext.HashedElGamalCiphertextHandle handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// Decrypt the ciphertext directly using the provided secret key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public byte[] Decrypt(ElementModQ secretKey, ElementModQ descriptionHash, bool lookForPadding)
        {
            var status = NativeInterface.HashedElGamalCiphertext.Decrypt(
                Handle, secretKey.Handle, descriptionHash.Handle, lookForPadding, out IntPtr data, out ulong size);
            status.ThrowIfError();

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);

            return byteArray;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }
    }
}