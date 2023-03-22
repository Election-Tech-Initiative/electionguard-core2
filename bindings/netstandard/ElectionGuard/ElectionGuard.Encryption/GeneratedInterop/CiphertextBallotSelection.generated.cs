// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace ElectionGuard
{
    public partial class CiphertextBallotSelection
    {
        internal External.CiphertextBallotSelectionHandle Handle;

        #region Properties

        /// <Summary>
        /// Get the objectId of the selection, which is the unique id for the selection in a specific contest described in the election manifest.
        /// </Summary>
        public string ObjectId
        {
            get
            {
                var status = External.GetObjectId(Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Get the sequence order of the selection
        /// </Summary>
        public ulong SequenceOrder
        {
            get
            {
                return External.GetSequenceOrder(Handle);
            }
        }

        /// <Summary>
        /// Determines if this is a placeholder selection
        /// </Summary>
        public bool IsPlaceholder
        {
            get
            {
                return External.GetIsPlaceholder(Handle);
            }
        }

        /// <Summary>
        /// The hash of the string representation of the Selection Description from the election manifest
        /// </Summary>
        public ElementModQ DescriptionHash
        {
            get
            {
                var status = External.GetDescriptionHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// The encrypted representation of the vote field
        /// </Summary>
        public ElGamalCiphertext Ciphertext
        {
            get
            {
                var status = External.GetCiphertext(
                    Handle, out NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElGamalCiphertext(value);
            }
        }

        /// <Summary>
        /// The hash of the encrypted values
        /// </Summary>
        public ElementModQ CryptoHash
        {
            get
            {
                var status = External.GetCryptoHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// The nonce used to generate the encryption. Sensitive &amp; should be treated as a secret
        /// </Summary>
        public ElementModQ Nonce
        {
            get
            {
                var status = External.GetNonce(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// The proof that demonstrates the selection is an encryption of 0 or 1, and was encrypted using the `nonce`
        /// </Summary>
        public DisjunctiveChaumPedersenProof Proof
        {
            get
            {
                var status = External.GetProof(
                    Handle, out NativeInterface.DisjunctiveChaumPedersenProof.DisjunctiveChaumPedersenProofHandle value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new DisjunctiveChaumPedersenProof(value);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Given an encrypted BallotSelection, generates a hash, suitable for rolling up into a hash / tracking code for an entire ballot. Of note, this particular hash examines the `encryptionSeed` and `message`, but not the proof. This is deliberate, allowing for the possibility of ElectionGuard variants running on much more limited hardware, wherein the Disjunctive Chaum-Pedersen proofs might be computed later on. In most cases the encryption_seed should match the `description_hash`.
        /// </summary>
        /// <param name="encryptionSeed">In most cases the encryption_seed should match the `description_hash`</param>
        public ElementModQ CryptoHashWith(
            ElementModQ encryptionSeed
        )
        {
            var status = External.CryptoHashWith(
                Handle,
                encryptionSeed.Handle,
                out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Given an encrypted BallotSelection, validates the encryption state against a specific seed hash and public key. Calling this function expects that the object is in a well-formed encrypted state with the elgamal encrypted `message` field populated along with the DisjunctiveChaumPedersenProof`proof` populated. the ElementModQ `description_hash` and the ElementModQ `crypto_hash` are also checked.
        /// </summary>
        /// <param name="encryptionSeed">The hash of the SelectionDescription, or whatever `ElementModQ` was used to populate the `description_hash` field.</param>
        /// <param name="elGamalPublicKey">The election public key.</param>
        /// <param name="cryptoExtendedBaseHash">The extended base hash of the election.</param>
        public bool IsValidEncryption(
            ElementModQ encryptionSeed, ElementModP elGamalPublicKey, ElementModQ cryptoExtendedBaseHash
        )
        {
            return External.IsValidEncryption(
                Handle, encryptionSeed.Handle, elGamalPublicKey.Handle, cryptoExtendedBaseHash.Handle);
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
        #endregion

        #region Extern

        internal static unsafe class External
        {
            internal struct CiphertextBallotSelectionType { };

            internal class CiphertextBallotSelectionHandle : ElectionGuardSafeHandle<CiphertextBallotSelectionType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    // releasing the C++ memory is currently handled by a parent object e.g. ballot, see https://github.com/microsoft/electionguard-core2/issues/29
                    return true;
                }
            }

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_free",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Free(CiphertextBallotSelectionType* handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_object_id",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetObjectId(
                CiphertextBallotSelectionHandle handle,
                out IntPtr objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_sequence_order",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetSequenceOrder(
                CiphertextBallotSelectionHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_is_placeholder",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern bool GetIsPlaceholder(
                CiphertextBallotSelectionHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_description_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetDescriptionHash(
                CiphertextBallotSelectionHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_ciphertext",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetCiphertext(
                CiphertextBallotSelectionHandle handle,
                out NativeInterface.ElGamalCiphertext.ElGamalCiphertextHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_crypto_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetCryptoHash(
                CiphertextBallotSelectionHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_nonce",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetNonce(
                CiphertextBallotSelectionHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_proof",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetProof(
                CiphertextBallotSelectionHandle handle,
                out NativeInterface.DisjunctiveChaumPedersenProof.DisjunctiveChaumPedersenProofHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_crypto_hash_with",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status CryptoHashWith(
                CiphertextBallotSelectionHandle handle,
                NativeInterface.ElementModQ.ElementModQHandle encryptionSeed,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_is_valid_encryption",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern bool IsValidEncryption(
                CiphertextBallotSelectionHandle handle,
                NativeInterface.ElementModQ.ElementModQHandle encryptionSeed,
                NativeInterface.ElementModP.ElementModPHandle elGamalPublicKey,
                NativeInterface.ElementModQ.ElementModQHandle cryptoExtendedBaseHash
                );

        }
        #endregion
    }
}
