using System;
using System.Runtime.InteropServices;
// ReSharper disable RedundantUnsafeContext
// ReSharper disable UnusedMember.Global

namespace ElectionGuard
{
    using NativeDisjunctiveChaumPedersenProof = NativeInterface.DisjunctiveChaumPedersenProof.DisjunctiveChaumPedersenProofHandle;
    // Declare native types for convenience
    using NativeExtendedData = NativeInterface.ExtendedData.ExtendedDataHandle;

    #region ExtendedData

    /// <summary>
    /// ExtendedData represents any arbitrary data expressible as a string with a length.
    ///
    /// This class is used primarily as a field on a selection to indicate a write-in candidate text value
    /// </summary>
    public class ExtendedData : DisposableBase
    {
        /// <Summary>
        /// The string value
        /// </Summary>
        public unsafe string Value
        {
            get
            {
                var status = NativeInterface.ExtendedData.GetValue(
                    Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The length of the string value
        /// </Summary>
        public unsafe ulong Length => NativeInterface.ExtendedData.GetLength(Handle);

        internal unsafe NativeExtendedData Handle;

        internal unsafe ExtendedData(NativeExtendedData handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Creates an ExtendedData object
        /// </summary>
        /// <param name="value">the string value</param>
        /// <param name="length">the length of the string</param>
        public unsafe ExtendedData(string value, ulong length)
        {
            var status = NativeInterface.ExtendedData.New(
                value, length, out Handle);
            status.ThrowIfError();
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override unsafe void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }

    }

    #endregion

    #region PlaintextBallotSelection

    #endregion

    #region CiphertextBallotSelection

    #endregion

    #region PlaintextBallotContest

    #endregion

    #region CiphertextBallotContest

    #endregion

    #region PlaintextBallot

    #endregion

    #region CompactPlaintextBallot

    #endregion

    #region CiphertextBallot

    #endregion

    #region CompactCiphertextBallot

    #endregion

    #region SubmittedBallot

    #endregion

}
