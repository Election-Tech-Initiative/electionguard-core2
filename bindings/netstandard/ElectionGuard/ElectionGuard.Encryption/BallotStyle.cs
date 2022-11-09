using System;
using System.Runtime.InteropServices;
// ReSharper disable UnusedMember.Global

namespace ElectionGuard
{
    /// <Summary>
    /// A BallotStyle works as a key to uniquely specify a set of contests. See also `ContestDescription`.
    ///
    /// For ElectionGuard, each contest is associated with a specific geopolitical unit,
    /// and each ballot style is associated with at least one geopolitical unit.
    ///
    /// It is up to the consuming application to determine how to interpret the relationships
    /// between these entity types.
    /// </Summary>
    public class BallotStyle : DisposableBase
    {
        /// <Summary>
        /// Unique internal identifier that's used by other elements to reference this element.
        /// </Summary>
        public unsafe string ObjectId
        {
            get
            {
                var status = NativeInterface.BallotStyle.GetObjectId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"BallotStyle Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// the size of the geopolitical unit id collection
        /// </Summary>
        public unsafe ulong GeopoliticalUnitIdsSize
        {
            get
            {
                var size = NativeInterface.BallotStyle.GetGeopoliticalUnitSize(
                    Handle);
                return size;
            }
        }

        /// <Summary>
        /// the size of the party id collection
        /// </Summary>
        public unsafe ulong PartyIdsSize
        {
            get
            {
                var size = NativeInterface.BallotStyle.GetPartyIdsSize(
                    Handle);
                return size;
            }
        }

        internal unsafe NativeInterface.BallotStyle.BallotStyleHandle Handle;

        internal unsafe BallotStyle(
            NativeInterface.BallotStyle.BallotStyleHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `BallotStyle` object
        /// </summary>
        /// <param name="objectId">string to identify the `BallotStyle`</param>
        /// <param name="gpUnitIds">array of objectIds for the `GeopoliticalUnit` for this `BallotStyle`</param>
        public unsafe BallotStyle(string objectId, string[] gpUnitIds)
        {
            var status = NativeInterface.BallotStyle.New(objectId, gpUnitIds, (ulong)gpUnitIds.Length, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"BallotStyle Error Status: {status}");
            }
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

        /// <Summary>
        /// the Geopolitical Unit Id or id's that correlate to this ballot style
        /// </Summary>
        public unsafe String GetGeopoliticalUnitIdAt(ulong index)
        {
            var status = NativeInterface.BallotStyle.GetGeopoliticalInitIdAtIndex(
                Handle, index, out IntPtr value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"BallotStyle Error GetGeopoliticalUnitIdAt: {status}");
            }
            var data = Marshal.PtrToStringAnsi(value);
            NativeInterface.Memory.FreeIntPtr(value);
            return data;
        }

        /// <Summary>
        /// the Party Id or Id's (if any) for this ballot style
        /// </Summary>
        public unsafe String GetPartyIdAt(ulong index)
        {
            var status = NativeInterface.BallotStyle.GetPartyIdAtIndex(
                Handle, index, out IntPtr value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"BallotStyle Error GetPartyIdAt: {status}");
            }
            var data = Marshal.PtrToStringAnsi(value);
            NativeInterface.Memory.FreeIntPtr(value);
            return data;
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public unsafe ElementModQ CryptoHash()
        {
            var status = NativeInterface.BallotStyle.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}