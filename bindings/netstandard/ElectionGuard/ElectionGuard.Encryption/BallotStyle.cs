using System.Collections.Generic;
using System.Runtime.InteropServices;

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
        public string ObjectId
        {
            get
            {
                var status = NativeInterface.BallotStyle.GetObjectId(
                    Handle, out var value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"BallotStyle Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                _ = NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The collection of geopolitical unit ids for this ballot style
        /// </Summary>
        public IReadOnlyList<string> GeopoliticalUnitIds =>
            new ElectionGuardEnumerator<string>(
                () => (int)GeopoliticalUnitIdsSize,
                (index) => GetGeopoliticalUnitIdAtIndex((ulong)index)
            );

        /// <Summary>
        /// The collection of party ids for this ballot style
        /// </Summary>
        public IReadOnlyList<string> PartyIds =>
            new ElectionGuardEnumerator<string>(
                () => (int)PartyIdsSize,
                (index) => GetPartyIdAtIndex((ulong)index)
            );

        /// <Summary>
        /// the size of the geopolitical unit id collection
        /// </Summary>
        public ulong GeopoliticalUnitIdsSize
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
        public ulong PartyIdsSize
        {
            get
            {
                var size = NativeInterface.BallotStyle.GetPartyIdsSize(
                    Handle);
                return size;
            }
        }

        internal NativeInterface.BallotStyle.BallotStyleHandle Handle;

        internal BallotStyle(
            NativeInterface.BallotStyle.BallotStyleHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `BallotStyle` object
        /// </summary>
        /// <param name="objectId">string to identify the `BallotStyle`</param>
        /// <param name="gpUnitIds">array of objectIds for the `GeopoliticalUnit` for this `BallotStyle`</param>
        public BallotStyle(string objectId, string[] gpUnitIds)
        {
            var status = NativeInterface.BallotStyle.New(objectId, gpUnitIds, (ulong)gpUnitIds.Length, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"BallotStyle Error Status: {status}");
            }
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

        /// <Summary>
        /// the Geopolitical Unit Id or id's that correlate to this ballot style
        /// </Summary>
        public string GetGeopoliticalUnitIdAtIndex(ulong index)
        {
            var status = NativeInterface.BallotStyle.GetGeopoliticalInitIdAtIndex(
                Handle, index, out var value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"BallotStyle Error GetGeopoliticalUnitIdAt: {status}");
            }
            var data = Marshal.PtrToStringAnsi(value);
            _ = NativeInterface.Memory.FreeIntPtr(value);
            return data;
        }

        /// <Summary>
        /// the Party Id or Id's (if any) for this ballot style
        /// </Summary>
        public string GetPartyIdAtIndex(ulong index)
        {
            var status = NativeInterface.BallotStyle.GetPartyIdAtIndex(
                Handle, index, out var value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"BallotStyle Error GetPartyIdAt: {status}");
            }
            var data = Marshal.PtrToStringAnsi(value);
            _ = NativeInterface.Memory.FreeIntPtr(value);
            return data;
        }

        /// <Summary>
        /// A hash representation of the object
        /// </Summary>
        public ElementModQ CryptoHash()
        {
            var status = NativeInterface.BallotStyle.CryptoHash(
                Handle, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"CryptoHash Error Status: {status}")
                : new ElementModQ(value);
        }
    }
}
