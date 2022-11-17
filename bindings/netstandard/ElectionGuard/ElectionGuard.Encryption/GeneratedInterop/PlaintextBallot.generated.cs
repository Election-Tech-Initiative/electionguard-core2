// DO NOT MODIFY THIS FILE
// This file is generated via ElectionGuard.InteropGenerator at /src/interop-generator

using System;
using System.Runtime.InteropServices;
using System.Runtime.ConstrainedExecution;

namespace ElectionGuard
{
    public partial class PlaintextBallot
    {
        internal External.PlaintextBallotHandle Handle;

        #region Properties

        /// <Summary>
        /// A unique Ballot ID that is relevant to the external system and must be unique within the dataset of the election.
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
        /// The Object Id of the ballot style in the election manifest.  This value is used to determine which contests to expect on the ballot, to fill in missing values, and to validate that the ballot is well-formed.
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
        /// The size of the Contests collection.
        /// </Summary>
        public ulong ContestsSize
        {
            get
            {
                return External.GetContestsSize(Handle);
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get the contest at the specified index.
        /// </summary>
        /// <param name="index">The index of the contest</param>
        public PlaintextBallotContest GetContestAtIndex(
            ulong index
        ) {
            var status = External.GetContestAtIndex(
                Handle,
                index,
                out PlaintextBallotContest.External.PlaintextBallotContestHandle value);
            status.ThrowIfError();
            return new PlaintextBallotContest(value);
        }

        /// <summary>
        /// Export the ballot representation as JSON
        /// </summary>
        public string ToJson(
            
        ) {
            var status = External.ToJson(
                Handle,
                out IntPtr pointer, 
                out _
                );
            status.ThrowIfError();
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        /// <summary>
        /// Export the ballot representation as BSON
        /// </summary>
        public byte[] ToBson(
            
        ) {
            var status = External.ToBson(
                Handle,
                out IntPtr data, 
                out ulong size
                );
            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("PlaintextBallot Error ToBson: size is too big");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }

        /// <summary>
        /// Export the ballot representation as MsgPack
        /// </summary>
        public byte[] ToMsgPack(
            
        ) {
            var status = External.ToMsgPack(
                Handle,
                out IntPtr data, 
                out ulong size
                );
            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("PlaintextBallot Error ToMsgPack: size is too big");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
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

        internal static unsafe class External {
            internal struct PlaintextBallotType { };

            internal class PlaintextBallotHandle : ElectionGuardSafeHandle<PlaintextBallotType>
            {
                [ReliabilityContract(Consistency.WillNotCorruptState, Cer.MayFail)]
                protected override bool Free()
                {
                    if (IsFreed) return true;

                    var status = External.Free(TypedPtr);
                    if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                    {
                        throw new ElectionGuardException($"PlaintextBallot Error Free: {status}", status);
                    }
                    return true;
                }
            }

            [DllImport(
                NativeInterface.DllName, 
                EntryPoint = "eg_plaintext_ballot_free",
                CallingConvention = CallingConvention.Cdecl, 
                SetLastError = true)]
            internal static extern Status Free(PlaintextBallotType* handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_get_object_id",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetObjectId(
                PlaintextBallotHandle handle,
                out IntPtr objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_get_style_id",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetStyleId(
                PlaintextBallotHandle handle,
                out IntPtr objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_get_contests_size",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern ulong GetContestsSize(
                PlaintextBallotHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_get_contest_at_index",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status GetContestAtIndex(
                PlaintextBallotHandle handle,
                ulong index,
                out PlaintextBallotContest.External.PlaintextBallotContestHandle objectId
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_to_json",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status ToJson(
                PlaintextBallotHandle handle,
                out IntPtr data,
                out ulong size
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_to_bson",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status ToBson(
                PlaintextBallotHandle handle,
                out IntPtr data,
                out ulong size
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_plaintext_ballot_to_msg_pack",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status ToMsgPack(
                PlaintextBallotHandle handle,
                out IntPtr data,
                out ulong size
                );

        }
        #endregion
    }
}
