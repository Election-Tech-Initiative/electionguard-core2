using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// A "Hashed ElGamal Ciphertext" as specified as the Auxiliary Encryption in
    /// the ElectionGuard specification. The tuple g ^ r mod p concatenated with
    /// K ^ r mod p are used to feed into a hash function to generate a main key
    /// from which other keys derive to perform XOR encryption and to MAC the
    /// result. Create one with `hashedElgamalEncrypt`. Decrypt using one of the
    /// 'decrypt' methods.
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
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ElementModP(value);
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
                    Handle, out var data, out var size);
                status.ThrowIfError();
                var byteArray = new byte[(int)size];
                Marshal.Copy(data, byteArray, 0, (int)size);
                _ = NativeInterface.Memory.DeleteIntPtr(data);
                return byteArray;
            }
        }
        /// <summary>
        /// Message Authentication Code value
        /// </summary>
        public byte[] Mac
        {
            get
            {
                var status = NativeInterface.HashedElGamalCiphertext.GetMac(
                    Handle, out var data, out var size);
                status.ThrowIfError();
                var byteArray = new byte[(int)size];
                Marshal.Copy(data, byteArray, 0, (int)size);
                _ = NativeInterface.Memory.DeleteIntPtr(data);
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
                    Handle, out var value);
                status.ThrowIfError();
                return value.IsInvalid ? null : new ElementModQ(value);
            }
        }

        internal NativeInterface.HashedElGamalCiphertext.HashedElGamalCiphertextHandle Handle;

        public HashedElGamalCiphertext(HashedElGamalCiphertext other) : this(
            new ElementModP(other.Pad), other.Data, other.Mac)
        {
        }

        public unsafe HashedElGamalCiphertext(ElementModP pad, byte[] data, byte[] mac)
        {
            fixed (byte* _data = data)
            {
                fixed (byte* _mac = mac)
                {
                    var status = NativeInterface.HashedElGamalCiphertext.New(
                        pad.Handle,
                        _data,
                        (ulong)data.Length,
                        _mac,
                        (ulong)mac.Length,
                        out Handle);
                    status.ThrowIfError();
                }
            }
        }

        internal HashedElGamalCiphertext(
            NativeInterface.HashedElGamalCiphertext.HashedElGamalCiphertextHandle handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// Decrypt the ciphertext directly using the provided secret key.
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should not be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public byte[] Decrypt(
            ElementModP publicKey,
            ElementModQ secretKey, string hashPrefix,
            ElementModQ encryptionSeed, bool lookForPadding)
        {
            if (Handle == null || Handle.IsInvalid)
            {
                throw new ObjectDisposedException(nameof(HashedElGamalCiphertext));
            }

            if (secretKey == null || !secretKey.IsInBounds())
            {
                throw new ArgumentNullException(nameof(secretKey));
            }
            if (encryptionSeed == null || !encryptionSeed.IsInBounds())
            {
                throw new ArgumentNullException(nameof(encryptionSeed));
            }

            var status = NativeInterface.HashedElGamalCiphertext.Decrypt(
                Handle, publicKey.Handle, secretKey.Handle, hashPrefix,
                encryptionSeed.Handle, lookForPadding, out var data, out var size);
            status.ThrowIfError();

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            _ = NativeInterface.Memory.DeleteIntPtr(data);

            return byteArray;
        }

        /// <Summary>
        /// Partially Decrypts an ElGamal ciphertext with a known ElGamal secret key.
        ///
        /// 𝑀_i = C0^𝑠𝑖 mod 𝑝 in the spec
        ///
        /// This is a convenience accessor useful for some use cases.
        /// This method should be used by consumers operating in live secret ballot elections.
        /// </Summary>
        public ElementModP PartialDecrypt(ElementModQ secretKey)
        {
            var status = NativeInterface.HashedElGamalCiphertext.PartialDecrypt(
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
    }
}
