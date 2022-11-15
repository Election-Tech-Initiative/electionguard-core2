using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// `InternalManifest` is a subset of the `Manifest` structure that specifies
    /// the components that ElectionGuard uses for conducting an election.  The key component is the
    /// `contests` collection, which applies placeholder selections to the `Manifest` contests
    /// </summary>
    public class InternalManifest : DisposableBase
    {
        /// <summary>
        /// The hash of the election manifest
        /// </summary>
        public ElementModQ ManifestHash
        {
            get
            {
                var status = NativeInterface.InternalManifest.GetManifestHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"ManifestHash Error Status: {status}");
                }
                return new ElementModQ(value);
            }
        }

        /// <Summary>
        /// The size of the geopolitical units collection
        /// </Summary>
        public ulong GeopoliticalUnitsSize
        {
            get
            {
                var value = NativeInterface.InternalManifest.GetGeopoliticalUnitsSize(Handle);
                return value;
            }
        }

        /// <Summary>
        /// The size of the contests collection
        /// </Summary>
        public ulong ContestsSize
        {
            get
            {
                var value = NativeInterface.InternalManifest.GetContestsSize(Handle);
                return value;
            }
        }

        /// <Summary>
        /// The size of the ballot styles collection
        /// </Summary>
        public ulong BallotStylesSize
        {
            get
            {
                var value = NativeInterface.InternalManifest.GetBallotStylesSize(Handle);
                return value;
            }
        }

        internal NativeInterface.InternalManifest.InternalManifestHandle Handle;

        /// <summary>
        /// Creates an `InternalManifest` object
        /// </summary>
        /// <param name="manifest">public manifest to copy</param>
        public InternalManifest(Manifest manifest)
        {
            var status = NativeInterface.InternalManifest.New(manifest.Handle, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternalManifest Error Status: {status}");
            }
        }

        /// <summary>
        /// Creates an `InternalManifest` object
        /// </summary>
        /// <param name="json">string of json data describing the manifest</param>
        public InternalManifest(string json)
        {
            var status = NativeInterface.InternalManifest.FromJson(json, out Handle);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternalManifest Error Status: {status}");
            }
        }

        /// <summary>
        /// Creates an `InternalManifest` object
        /// </summary>
        /// <param name="data">byte array of data describing the manifest</param>
        /// <param name="encoding">binary encoding for the data</param>
        public unsafe InternalManifest(byte[] data, BinarySerializationEncoding encoding)
        {
            fixed (byte* pointer = data)
            {
                var status = encoding == BinarySerializationEncoding.BSON
                    ? NativeInterface.InternalManifest.FromBson(pointer, (ulong)data.Length, out Handle)
                    : NativeInterface.InternalManifest.FromMsgPack(pointer, (ulong)data.Length, out Handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"InternalManifest Error Binary Ctor: {status}");
                }
            }
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

        /// <Summary>
        /// Collection of geopolitical units for this election.
        /// </Summary>
        public GeopoliticalUnit GetGeopoliticalUnitAtIndex(ulong index)
        {
            var status = NativeInterface.InternalManifest.GetGeopoliticalUnitAtIndex(
                Handle, index, out NativeInterface.GeopoliticalUnit.GeopoliticalUnitHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternalManifest Error GetGeopoliticalUnitAtIndex: {status}");
            }
            return new GeopoliticalUnit(value);
        }

        /// <Summary>
        /// Collection of contests for this election.
        /// </Summary>
        public ContestDescription GetContestAtIndex(ulong index)
        {
            var status = NativeInterface.InternalManifest.GetContestAtIndex(
                Handle, index, out NativeInterface.ContestDescription.ContestDescriptionHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternalManifest Error GetContestAtIndex: {status}");
            }
            return new ContestDescription(value);
        }

        /// <Summary>
        /// Collection of ballot styles for this election.
        /// </Summary>
        public BallotStyle GetBallotStyleAtIndex(ulong index)
        {
            var status = NativeInterface.InternalManifest.GetBallotStyleAtIndex(
                Handle, index, out NativeInterface.BallotStyle.BallotStyleHandle value);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternalManifest Error GetContestAtIndex: {status}");
            }
            return new BallotStyle(value);
        }

        /// <Summary>
        /// Export the ballot representation as JSON
        /// </Summary>
        public string ToJson()
        {
            var status = NativeInterface.InternalManifest.ToJson(
                Handle, out IntPtr pointer, out _);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ToJson Error Status: {status}");
            }
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToBson()
        {

            var status = NativeInterface.InternalManifest.ToBson(
                Handle, out IntPtr data, out ulong size);

            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternalManifest Error ToBson: {status}");
            }

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException($"InternalManifest Error ToBson: size is too big. Expected <= {int.MaxValue}, actual: {size}.");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToMsgPack()
        {

            var status = NativeInterface.InternalManifest.ToMsgPack(
                Handle, out IntPtr data, out ulong size);

            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"InternalManifest Error ToMsgPack: {status}");
            }

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException($"InternalManifest Error ToMsgPack: size is too big. Expected <= {int.MaxValue}, actual: {size}.");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }
    }
}