// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    public partial class PlaintextBallotContest
    {
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

        /// <Summary>
        /// Get the Size of the selections collection
        /// </Summary>
        public unsafe ulong SelectionsSize
        {
            get
            {
                return External.GetSelectionsSize(Handle);
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// Given a PlaintextBallotContest returns true if the state is representative of the expected values.  Note: because this class supports partial representations, undervotes are considered a valid state.
        /// </summary>
        public unsafe bool IsValid(
            string expectedObjectId, ulong expectedNumSelections, ulong expectedNumElected, ulong votesAllowed = 0
        ) {
            return External.IsValid(
                Handle, expectedObjectId, expectedNumSelections, expectedNumElected, votesAllowed);
        }
        #endregion

        #region Extern
        private unsafe static class External {
            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_contest_get_object_id",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true
            )]
            internal static extern Status GetObjectId(
                NativeInterface.PlaintextBallotContest.PlaintextBallotContestHandle handle
                , out IntPtr objectId
            );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_contest_get_selections_size",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true
            )]
            internal static extern ulong GetSelectionsSize(
                NativeInterface.PlaintextBallotContest.PlaintextBallotContestHandle handle
            );

            [DllImport(NativeInterface.DllName, EntryPoint = "eg_plaintext_ballot_contest_is_valid",
                CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            internal static extern bool IsValid(
                NativeInterface.PlaintextBallotContest.PlaintextBallotContestHandle handle,
                [MarshalAs(UnmanagedType.LPStr)] string expectedObjectId,
                ulong expectedNumSelections,
                ulong expectedNumElected,
                ulong votesAllowed
                );
        }
        #endregion
    }
}
