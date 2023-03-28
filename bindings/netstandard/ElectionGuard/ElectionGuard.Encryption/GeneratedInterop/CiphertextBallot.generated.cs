// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace ElectionGuard
{
    public partial class CiphertextBallot
    {
        internal External.CiphertextBallotHandle Handle;

        #region Properties

        /// <Summary>
        /// The unique ballot id that is meaningful to the consuming application.
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
        /// The Object Id of the ballot style in the election manifest.  This value is used to determine which contests to expect on the ballot, to fill in missing values, and to validate that the ballot is well-formed
        /// </Summary>
        public string StyleId
        {
            get
            {
                var status = External.GetStyleId(Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Hash of the complete Election Manifest to which this ballot belongs
        /// </Summary>
        public ElementModQ ManifestHash
        {
            get
            {
                var status = External.GetManifestHash(
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
        /// The seed hash for the ballot.  It may be the encryption device hash, the hash of a previous ballot or the hash of some other value that is meaningful to the consuming application.
        /// </Summary>
        public ElementModQ BallotCodeSeed
        {
            get
            {
                var status = External.GetBallotCodeSeed(
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
        /// Get the size of the contests collection
        /// </Summary>
        public ulong ContestsSize
        {
            get
            {
                return External.GetContestsSize(Handle);
            }
        }

        /// <Summary>
        /// The unique ballot code for this ballot that is derived from the ballot seed, the timestamp, and the hash of the encrypted values
        /// </Summary>
        public ElementModQ BallotCode
        {
            get
            {
                var status = External.GetBallotCode(
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
        /// The timestamp indicating when the ballot was encrypted as measured by the encryption device.  This value does not provide units as it is up to the host system to indicate the scale. Typically a host may use seconds since epoch or ticks since epoch.
        /// </Summary>
        public ulong Timestamp
        {
            get
            {
                return External.GetTimestamp(Handle);
            }
        }

        /// <Summary>
        /// The nonce value used to encrypt all values in the ballot
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

        #endregion

        #region Methods

        /// <summary>
        /// Get a contest at a specific index
        /// </summary>
        /// <param name="index">The index of the contest</param>
        public CiphertextBallotContest GetContestAtIndex(
            ulong index
        )
        {
            var status = External.GetContestAtIndex(
                Handle,
                index,
                out CiphertextBallotContest.External.CiphertextBallotContestHandle value);
            status.ThrowIfError();
            return new CiphertextBallotContest(value);
        }

        /// <summary>
        /// Given an encrypted Ballot, validates the encryption state against a specific ballot seed and public key by verifying the states of this ballot's members (BallotContest's and BallotSelection's). Calling this function expects that the object is in a well-formed encrypted state with the `contests` populated with valid encrypted ballot selections, and the ElementModQ `manifest_hash` also populated. Specifically, the seed in this context is the hash of the Election Manifest, or whatever `ElementModQ` was used to populate the `manifest_hash` field.
        /// </summary>
        public bool IsValidEncryption(
            ElementModQ manifestHash, ElementModP elGamalPublicKey, ElementModQ cryptoExtendedBaseHash
        )
        {
            return External.IsValidEncryption(
                Handle, manifestHash.Handle, elGamalPublicKey.Handle, cryptoExtendedBaseHash.Handle);
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
            internal struct CiphertextBallotType { };

            internal class CiphertextBallotHandle : ElectionGuardSafeHandle<CiphertextBallotType>
            {
#if NETSTANDARD
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
#endif
                protected override bool Free()
                {
                    if (IsFreed) return true;

                    var status = External.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"CiphertextBallot Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_free",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status Free(CiphertextBallotType* handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_object_id",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetObjectId(
                CiphertextBallotHandle handle,
                out IntPtr objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_style_id",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetStyleId(
                CiphertextBallotHandle handle,
                out IntPtr objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_manifest_hash",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetManifestHash(
                CiphertextBallotHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_ballot_code_seed",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetBallotCodeSeed(
                CiphertextBallotHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_contests_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetContestsSize(
                CiphertextBallotHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_ballot_code",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetBallotCode(
                CiphertextBallotHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_timestamp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetTimestamp(
                CiphertextBallotHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_nonce",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetNonce(
                CiphertextBallotHandle handle,
                out NativeInterface.ElementModQ.ElementModQHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_get_contest_at_index",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetContestAtIndex(
                CiphertextBallotHandle handle,
                ulong index,
                out CiphertextBallotContest.External.CiphertextBallotContestHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_is_valid_encryption",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern bool IsValidEncryption(
                CiphertextBallotHandle handle,
                NativeInterface.ElementModQ.ElementModQHandle manifestHash,
                NativeInterface.ElementModP.ElementModPHandle elGamalPublicKey,
                NativeInterface.ElementModQ.ElementModQHandle cryptoExtendedBaseHash
                );

        }
        #endregion
    }
}
