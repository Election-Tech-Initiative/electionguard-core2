using System.Collections.Generic;
using System.Linq;
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
        public IReadOnlyList<GeopoliticalUnit> GeopoliticalUnits =>
            new ElectionGuardEnumerator<GeopoliticalUnit>(
                () => (int)GeopoliticalUnitsSize,
                (index) => GetGeopoliticalUnitAtIndex((ulong)index)
            );

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
        public IReadOnlyList<ContestDescriptionWithPlaceholders> Contests =>
            new ElectionGuardEnumerator<ContestDescriptionWithPlaceholders>(
            () => (int)ContestsSize,
            (index) => GetContestAtIndex((ulong)index)
        );

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
        public IReadOnlyList<BallotStyle> BallotStyles =>
            new ElectionGuardEnumerator<BallotStyle>(
                () => (int)BallotStylesSize,
                (index) => GetBallotStyleAtIndex((ulong)index)
            );

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

        public InternalManifest(InternalManifest other) : this(other.ToJson())
        {
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

        public List<ContestDescriptionWithPlaceholders> GetContests(string ballotStyleId)
        {
            var ballotStyle = BallotStyles.FirstOrDefault(i => i.ObjectId == ballotStyleId);
            if (ballotStyle == null)
            {
                throw new ElectionGuardException($"InternalManifest Error GetContests: BallotStyle not found");
            }
            if (!ballotStyle.GeopoliticalUnitIds.Any())
            {
                throw new ElectionGuardException($"InternalManifest Error GetContests: BallotStyle has no geopolitical units");
            }

            var gpUnits = ballotStyle.GeopoliticalUnitIds.ToList();

            return Contests.Where(i => gpUnits.Contains(i.ElectoralDistrictId)).ToList();
        }

        public List<ContestDescriptionWithPlaceholders> GetContests(BallotStyle ballotStyle)
        {
            return GetContests(ballotStyle.ObjectId);
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
        public ContestDescriptionWithPlaceholders GetContestAtIndex(ulong index)
        {
            var status = NativeInterface.InternalManifest.GetContestAtIndex(
                Handle, index, out var value);
            return status != Status.ELECTIONGUARD_STATUS_SUCCESS
                ? throw new ElectionGuardException($"InternalManifest Error GetContestAtIndex: {status}")
                : new ContestDescriptionWithPlaceholders(value);
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

        #region Serialization

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

        #endregion
    }
}
