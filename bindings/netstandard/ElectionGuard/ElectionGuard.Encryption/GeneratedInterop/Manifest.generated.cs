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

        }
        #endregion
    }
}
