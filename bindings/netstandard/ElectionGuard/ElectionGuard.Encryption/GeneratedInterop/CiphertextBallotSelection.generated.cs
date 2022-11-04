// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace ElectionGuard
{
    public partial class CiphertextBallotSelection
    {
        internal unsafe External.CiphertextBallotSelectionHandle Handle;

        #region Properties
        /// <Summary>
        /// Get the objectId of the contest, which is the unique id for the contest in a specific ballot style described in the election manifest.
        /// </Summary>
        public unsafe string ObjectId
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

        #endregion

        #region Methods

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override unsafe void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }
        #endregion

        #region Extern
        internal unsafe static class External {
            internal unsafe struct CiphertextBallotSelectionType { };

            internal class CiphertextBallotSelectionHandle : ElectionguardSafeHandle<CiphertextBallotSelectionType>
            {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
                protected override bool Free()
                {
                    if (IsFreed) return true;

                    var status = External.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"CiphertextBallotSelection Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(NativeInterface.DllName, EntryPoint = "eg_ciphertext_ballot_selection_free",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern Status Free(CiphertextBallotSelectionType* handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_ciphertext_ballot_selection_get_object_id",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true
            )]
            internal static extern Status GetObjectId(
                CiphertextBallotSelectionHandle handle
                , out IntPtr objectId
            );

        }
        #endregion
    }
}
