// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace ElectionGuard
{
    public partial class Manifest
    {
        internal External.ManifestHandle Handle;

        #region Properties

        /// <Summary>
        /// Unique identifier for a GpUnit element. Associates the election with a reporting unit that represents the geographical scope of the election, such as a state or city.
        /// </Summary>
        public string ElectionScopeId
        {
            get
            {
                var status = External.GetElectionScopeId(Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// Enumerated type of election, such as partisan-primary or open-primary.
        /// </Summary>
        public ElectionType ElectionType
        {
            get
            {
                return External.GetElectionType(Handle);
            }
        }

        /// <Summary>
        /// The start date/time of the election.
        /// </Summary>
        public DateTime StartDate
        {
            get
            {
                var value = External.GetStartDate(Handle);
                return DateTimeOffset.FromUnixTimeMilliseconds((long)value).DateTime;
            }
        }

        /// <Summary>
        /// The end date/time of the election.
        /// </Summary>
        public DateTime EndDate
        {
            get
            {
                var value = External.GetEndDate(Handle);
                return DateTimeOffset.FromUnixTimeMilliseconds((long)value).DateTime;
            }
        }

        /// <Summary>
        /// The size of the geopolitical units collection
        /// </Summary>
        public ulong GeopoliticalUnitsSize
        {
            get
            {
                return External.GetGeopoliticalUnitsSize(Handle);
            }
        }

        /// <Summary>
        /// The size of the parties collection
        /// </Summary>
        public ulong PartiesSize
        {
            get
            {
                return External.GetPartiesSize(Handle);
            }
        }

        /// <Summary>
        /// The size of the candidates collection
        /// </Summary>
        public ulong CandidatesSize
        {
            get
            {
                return External.GetCandidatesSize(Handle);
            }
        }

        /// <Summary>
        /// The size of the contests collection
        /// </Summary>
        public ulong ContestsSize
        {
            get
            {
                return External.GetContestsSize(Handle);
            }
        }

        /// <Summary>
        /// The size of the ballot styles collection
        /// </Summary>
        public ulong BallotStylesSize
        {
            get
            {
                return External.GetBallotStylesSize(Handle);
            }
        }

        /// <Summary>
        /// The friendly name of the election
        /// </Summary>
        public InternationalizedText Name
        {
            get
            {
                var status = External.GetName(
                    Handle, out NativeInterface.InternationalizedText.InternationalizedTextHandle value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new InternationalizedText(value);
            }
        }

        /// <Summary>
        /// The contact information for the election
        /// </Summary>
        public ContactInformation ContactInfo
        {
            get
            {
                var status = External.GetContactInfo(
                    Handle, out NativeInterface.ContactInformation.ContactInformationHandle value);
                status.ThrowIfError();
                if (value.IsInvalid)
                {
                    return null;
                }
                return new ContactInformation(value);
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
            internal struct ManifestType { };

            internal class ManifestHandle : ElectionGuardSafeHandle<ManifestType>
            {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
                protected override bool Free()
                {
                    if (IsFreed) return true;

                    var status = External.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"Manifest Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(
                NativeInterface.DllName, 
                EntryPoint = "eg_election_manifest_free",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true)]
            internal static extern Status Free(ManifestType* handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_election_scope_id",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetElectionScopeId(
                ManifestHandle handle,
                out IntPtr objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_election_type",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ElectionType GetElectionType(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_start_date",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetStartDate(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_end_date",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetEndDate(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_geopolitical_units_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetGeopoliticalUnitsSize(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_parties_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetPartiesSize(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_candidates_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetCandidatesSize(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_contests_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetContestsSize(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_ballot_styles_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetBallotStylesSize(
                ManifestHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_name",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetName(
                ManifestHandle handle,
                out NativeInterface.InternationalizedText.InternationalizedTextHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_election_manifest_get_contact_info",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetContactInfo(
                ManifestHandle handle,
                out NativeInterface.ContactInformation.ContactInformationHandle objectId
                );

        }
        #endregion
    }
}
