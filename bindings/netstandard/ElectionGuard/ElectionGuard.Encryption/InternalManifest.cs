using System;
using System.Collections;
using System.Collections.Generic;
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
                    Handle, out var value);
                return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                    ? throw new ElectionGuardException($"ManifestHash Error Status: {status}")
                    : new ElementModQ(value);
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
        /// The collection of geopolitical units
        /// </Summary>
        public IReadOnlyList<GeopoliticalUnit> GeopoliticalUnits => new GeopoliticalUnitEnumerator(this);

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
        /// The collection of contests
        /// </Summary>
        public IReadOnlyList<ContestDescription> Contests => new ContestDescriptionEnumerator(this);

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

        /// <Summary>
        /// The collection of ballot styles
        /// </Summary>
        public IReadOnlyList<BallotStyle> BallotStyles => new BallotStyleEnumerator(this);

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
        /// Creates an <see cref="InternalManifest">InternalManifest</see> object from a <see href="https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1">[RFC-8259]</see> UTF-8 encoded JSON string
        /// </summary>
        /// <param name="json">A UTF-8 Encoded JSON data string</param>
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

            if (Handle == null || Handle.IsInvalid)
            {
                return;
            }

            Handle.Dispose();
            Handle = null;
        }

        /// <Summary>
        /// Collection of geopolitical units for this election.
        /// </Summary>
        public GeopoliticalUnit GetGeopoliticalUnitAtIndex(ulong index)
        {
            var status = NativeInterface.InternalManifest.GetGeopoliticalUnitAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"InternalManifest Error GetGeopoliticalUnitAtIndex: {status}")
                : new GeopoliticalUnit(value);
        }

        /// <Summary>
        /// Collection of contests for this election.
        /// </Summary>
        public ContestDescription GetContestAtIndex(ulong index)
        {
            var status = NativeInterface.InternalManifest.GetContestAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"InternalManifest Error GetContestAtIndex: {status}")
                : new ContestDescription(value);
        }

        /// <Summary>
        /// Collection of ballot styles for this election.
        /// </Summary>
        public BallotStyle GetBallotStyleAtIndex(ulong index)
        {
            var status = NativeInterface.InternalManifest.GetBallotStyleAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"InternalManifest Error GetContestAtIndex: {status}")
                : new BallotStyle(value);
        }

        /// <Summary>
        /// Export the ballot representation as JSON
        /// </Summary>
        public string ToJson()
        {
            var status = NativeInterface.InternalManifest.ToJson(
                Handle, out var pointer, out _);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ToJson Error Status: {status}");
            }
            var json = Marshal.PtrToStringAnsi(pointer);
            _ = NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToBson()
        {

            var status = NativeInterface.InternalManifest.ToBson(
                Handle, out var data, out var size);

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
            _ = NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToMsgPack()
        {

            var status = NativeInterface.InternalManifest.ToMsgPack(
                Handle, out var data, out var size);

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
            _ = NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }
    }

    /// <summary>
    /// A GeopoliticalUnitEnumerator is an IReadonlyList<GeopoliticalUnit> implementation
    /// that can be used to enumerate the geopolitical units in an InternalManifest
    /// </summary>
    public class GeopoliticalUnitEnumerator : DisposableBase, IReadOnlyList<GeopoliticalUnit>
    {
        private readonly InternalManifest _manifest;

        /// <summary>
        /// Constructs a new GeopoliticalUnitEnumerator
        /// </summary>
        /// <param name="manifest">The manifest to enumerate</param>
        public GeopoliticalUnitEnumerator(InternalManifest manifest)
        {
            _manifest = manifest;
        }

        /// <summary>
        /// Gets the number of geopolitical units in the manifest
        /// </summary>
        public int Count => (int)_manifest.GeopoliticalUnitsSize;

        /// <summary>
        /// Gets the geopolitical unit at the specified index
        /// </summary>
        /// <param name="index">The index of the geopolitical unit to get</param>
        /// <returns>The geopolitical unit at the specified index</returns>
        public GeopoliticalUnit this[int index] => _manifest.GetGeopoliticalUnitAtIndex((ulong)index);

        public IEnumerator<GeopoliticalUnit> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _manifest.GetGeopoliticalUnitAtIndex((ulong)i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// A ContestDescriptionEnumerator is an IReadonlyList<ContestDescription> implementation
    /// that can be used to enumerate the contest descriptions in an InternalManifest
    /// </summary>
    public class ContestDescriptionEnumerator : DisposableBase, IReadOnlyList<ContestDescription>
    {
        private readonly InternalManifest _manifest;

        /// <summary>
        /// Constructs a new ContestDescriptionEnumerator
        /// </summary>
        /// <param name="manifest">The manifest to enumerate</param>
        public ContestDescriptionEnumerator(InternalManifest manifest)
        {
            _manifest = manifest;
        }

        /// <summary>
        /// Gets the number of contest descriptions in the manifest
        /// </summary>
        public int Count => (int)_manifest.ContestsSize;

        /// <summary>
        /// Gets the contest description at the specified index
        /// </summary>
        /// <param name="index">The index of the contest description to get</param>
        /// <returns>The contest description at the specified index</returns>
        public ContestDescription this[int index] => _manifest.GetContestAtIndex((ulong)index);

        public IEnumerator<ContestDescription> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _manifest.GetContestAtIndex((ulong)i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// A BallotStyleEnumerator is an IReadonlyList<BallotStyle> implementation
    /// that can be used to enumerate the ballot styles in an InternalManifest
    /// </summary>
    public class BallotStyleEnumerator : DisposableBase, IReadOnlyList<BallotStyle>
    {
        private readonly InternalManifest _manifest;

        /// <summary>
        /// Constructs a new BallotStyleEnumerator
        /// </summary>
        /// <param name="manifest">The manifest to enumerate</param>
        public BallotStyleEnumerator(InternalManifest manifest)
        {
            _manifest = manifest;
        }

        /// <summary>
        /// Gets the number of ballot styles in the manifest
        /// </summary>
        public int Count => (int)_manifest.BallotStylesSize;

        /// <summary>
        /// Gets the ballot style at the specified index
        /// </summary>
        /// <param name="index">The index of the ballot style to get</param>
        /// <returns>The ballot style at the specified index</returns>
        public BallotStyle this[int index] => _manifest.GetBallotStyleAtIndex((ulong)index);

        public IEnumerator<BallotStyle> GetEnumerator()
        {
            for (var i = 0; i < Count; i++)
            {
                yield return _manifest.GetBallotStyleAtIndex((ulong)i);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
