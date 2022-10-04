// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    public partial class PlaintextBallotSelection
    {
        #region Properties
        /// <Summary>
        /// Get the objectId of the selection which is the unique id for the selection in a specific contest described in the election manifest.
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

        /// <Summary>
        /// Determines if this is a placeholder selection
        /// </Summary>
        public unsafe bool IsPlaceholder
        {
            get
            {
                return External.GetIsPlaceholder(Handle);
            }
        }

        /// <Summary>
        /// Get the plaintext vote
        /// </Summary>
        public unsafe ulong Vote
        {
            get
            {
                return External.GetVote(Handle);
            }
        }

        /// <Summary>
        /// An optional field of arbitrary data, such as the value of a write-in candidate
        /// </Summary>
        public unsafe ExtendedData ExtendedData
        {
            get
            {
                var status = External.GetExtendedData(
                    Handle, out NativeInterface.ExtendedData.ExtendedDataHandle value);
                status.ThrowIfError();
                return new ExtendedData(value);
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Given a PlaintextBallotSelection validates that the object matches an expected object and that the plaintext value can resolve to a valid representation
        /// </summary>
        public unsafe bool IsValid(
            string expectedObjectId
        ) {
            return External.IsValid(
                Handle, expectedObjectId);
        }
        #endregion

        #region Extern
        private unsafe static class External {
            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_selection_get_object_id",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true
            )]
            internal static extern Status GetObjectId(
                NativeInterface.PlaintextBallotSelection.PlaintextBallotSelectionHandle handle
                , out IntPtr objectId
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_selection_get_is_placeholder",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true
            )]
            internal static extern bool GetIsPlaceholder(
                NativeInterface.PlaintextBallotSelection.PlaintextBallotSelectionHandle handle
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_selection_get_vote",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true
            )]
            internal static extern ulong GetVote(
                NativeInterface.PlaintextBallotSelection.PlaintextBallotSelectionHandle handle
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_selection_get_extended_data",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true
            )]
            internal static extern Status GetExtendedData(
                NativeInterface.PlaintextBallotSelection.PlaintextBallotSelectionHandle handle
                , out NativeInterface.ExtendedData.ExtendedDataHandle objectId
            );

            [DllImport(NativeInterface.DllName, EntryPoint = "eg_plaintext_ballot_selection_is_valid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsValid(
                NativeInterface.PlaintextBallotSelection.PlaintextBallotSelectionHandle handle,
                [MarshalAs(UnmanagedType.LPStr)] string expectedObjectId
                );
        }
        #endregion
    }
}
