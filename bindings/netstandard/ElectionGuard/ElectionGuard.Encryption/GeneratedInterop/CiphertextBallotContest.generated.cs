// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace ElectionGuard
{
    public partial class CiphertextBallotContest
    {
        internal External.CiphertextBallotContestHandle Handle;

        #region Properties

        /// <Summary>
        /// Get the objectId of the contest, which is the unique id for the contest in a specific ballot described in the election manifest.
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
        /// Get the sequence order of the contest
        /// </Summary>
        public ulong SequenceOrder
        {
            get
            {
                return External.GetSequenceOrder(Handle);
            }
        }

        /// <Summary>
        /// The hash of the string representation of the Contest Description from the election manifest
        /// </Summary>
        public ElementModQ DescriptionHash
        {
            get
            {
                var status = External.GetDescriptionHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// Get the Size of the selections collection
        /// </Summary>
        public ulong SelectionsSize
        {
            get
            {
                return External.GetSelectionsSize(Handle);
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
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// The proof demonstrates the sum of the selections does not exceed the maximum available selections for the contest, and that the proof was generated with the nonce
        /// </Summary>
        public DisjunctiveChaumPedersenProof Proof
        {
            get
            {
                var status = External.GetProof(
                    Handle, out NativeInterface.DisjunctiveChaumPedersenProof.DisjunctiveChaumPedersenProofHandle value);
                status.ThrowIfError();
                return new DisjunctiveChaumPedersenProof(value);
            }
        }

        #endregion

        #region Methods


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

        internal static unsafe class External {
            internal struct CiphertextBallotContestType { };

            internal class CiphertextBallotContestHandle : ElectionGuardSafeHandle<CiphertextBallotContestType>
            {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
                protected override bool Free()
                {
                    // releasing the C++ memory is currently handled by a parent object e.g. ballot, see https://github.com/microsoft/electionguard-core2/issues/29
                    return true;
                }
            }

            [DllImport(
                NativeInterface.DllName, 
                EntryPoint = "eg_ciphertext_ballot_contest_free",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true)]
            internal static extern Status Free(CiphertextBallotContestType* handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_object_id",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetObjectId(
                CiphertextBallotContestHandle handle,
                out IntPtr objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_sequence_order",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetSequenceOrder(
                CiphertextBallotContestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_description_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetDescriptionHash(
                CiphertextBallotContestHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_selections_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetSelectionsSize(
                CiphertextBallotContestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_crypto_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetCryptoHash(
                CiphertextBallotContestHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_nonce",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetNonce(
                CiphertextBallotContestHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_contest_get_proof",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetProof(
                CiphertextBallotContestHandle handle,
                out NativeInterface.DisjunctiveChaumPedersenProof.DisjunctiveChaumPedersenProofHandle objectId
                );

        }
        #endregion
    }
}
