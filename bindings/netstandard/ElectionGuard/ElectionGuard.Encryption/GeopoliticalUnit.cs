using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <Summary>
    /// A geopolitical unit describes any physical or
    /// virtual unit of representation or vote/seat aggregation.
    /// Use this entity for defining geopolitical units such as
    /// cities, districts, jurisdictions, or precincts,
    /// for the purpose of associating contests, offices, vote counts,
    /// or other information with the geographies.
    ///
    /// Geopolitical Units are not used when encrypting ballots but are required by
    /// ElectionGuard to determine the validity of ballot styles.
    ///
    /// See: https://developers.google.com/elections-data/reference/gp-unit
    /// </Summary>
    public class GeopoliticalUnit : DisposableBase
    {
        /// <Summary>
        /// Unique internal identifier that's used by other elements to reference this element.
        /// </Summary>
        public unsafe string ObjectId
        {
            get
            {
                var status = NativeInterface.GeopoliticalUnit.GetObjectId(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"GeopoliticalUnit Error ObjectId: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Name of the geopolitical unit.
        /// </Summary>
        public unsafe string Name
        {
            get
            {
                var status = NativeInterface.GeopoliticalUnit.GetName(
                    Handle, out IntPtr value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"GeopoliticalUnit Error Name: {status}");
                }
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The type of reporting unit
        /// </Summary>
        public unsafe ReportingUnitType ReportingUnitType
        {
            get
            {
                var value = NativeInterface.GeopoliticalUnit.GetReportingUnitType(
                    Handle);

                return value;
            }
        }

        internal unsafe NativeInterface.GeopoliticalUnit.GeopoliticalUnitHandle Handle;

        internal unsafe GeopoliticalUnit(
            NativeInterface.GeopoliticalUnit.GeopoliticalUnitHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a `GeopoliticalUnit` object
        /// </summary>
        /// <param name="objectId">string to identify the unit</param>
        /// <param name="name">name of the unit</param>
        /// <param name="reportingUnitType">type of geopolitical unit</param>
        public unsafe GeopoliticalUnit(
            string objectId, string name, ReportingUnitType reportingUnitType)
        {
            var status = NativeInterface.GeopoliticalUnit.New(
                objectId, name, reportingUnitType, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ContactInformation Error Status: {status}");
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
        /// A hash representation of the object
        /// </Summary>
        public unsafe ElementModQ CryptoHash()
        {
            var status = NativeInterface.GeopoliticalUnit.CryptoHash(
                Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CryptoHash Error Status: {status}");
            }
            return new ElementModQ(value);
        }
    }
}