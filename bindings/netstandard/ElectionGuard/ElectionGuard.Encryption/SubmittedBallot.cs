using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// A `SubmittedBallot` represents a ballot that is submitted for inclusion in election results.
    /// A submitted ballot is or is about to be either cast or spoiled.
    /// The state supports the `BallotBoxState.UNKNOWN` enumeration to indicate that this object is mutable
    /// and has not yet been explicitly assigned a specific state.
    ///
    /// Note, additionally, this ballot includes all proofs but no nonces.
    ///
    /// Do not make this class directly. Use `make_ciphertext_submitted_ballot` or `from_ciphertext_ballot` instead.
    /// </summary>
    public class SubmittedBallot : DisposableBase
    {
        /// <summary>
        /// Get the BallotBoxState
        /// </summary>
        public BallotBoxState State => NativeInterface.SubmittedBallot.GetState(Handle);

        /// <summary>
        /// Get the ObjectId
        /// </summary>
        public string ObjectId
        {
            get
            {
                var status = NativeInterface.SubmittedBallot.GetObjectId(
                    Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <Summary>
        /// The Object Id of the ballot style in the election manifest.  This value is used
        /// to determine which contests to expect on the ballot, to fill in missing values,
        /// and to validate that the ballot is well-formed
        /// </Summary>
        public string StyleId
        {
            get
            {
                var status = NativeInterface.SubmittedBallot.GetStyleId(
                    Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <summary>
        /// Hash of the complete Election Manifest to which this ballot belongs
        /// </summary>
        public ElementModQ ManifestHash
        {
            get
            {
                var status = NativeInterface.SubmittedBallot.GetManifestHash(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// The seed hash for the ballot.  It may be the encryption device hash,
        /// the hash of a previous ballot or the hash of some other value
        /// that is meaningful to the consuming application.
        /// </summary>
        public ElementModQ BallotCodeSeed
        {
            get
            {
                var status = NativeInterface.SubmittedBallot.GetBallotCodeSeed(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// Get the size of the contests collection
        /// </summary>
        public ulong ContestsSize
        {
            get
            {
                var size = NativeInterface.SubmittedBallot.GetContestsSize(Handle);
                return size;
            }
        }

        /// <summary>
        /// The unique ballot code for this ballot that is derived from
        /// the ballot seed, the timestamp, and the hash of the encrypted values
        /// </summary>
        public ElementModQ BallotCode
        {
            get
            {
                var status = NativeInterface.SubmittedBallot.GetBallotCode(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// The timestamp indicating when the ballot was encrypted
        /// as measured by the encryption device.  This value does not
        /// provide units as it is up to the host system to indicate the scale.
        /// Typically a host may use seconds since epoch or ticks since epoch
        /// </summary>
        public ulong Timestamp
        {
            get
            {
                var value = NativeInterface.SubmittedBallot.GetTimestamp(Handle);
                return value;
            }
        }

        internal NativeInterface.SubmittedBallot.SubmittedBallotHandle Handle;

        internal SubmittedBallot(NativeInterface.SubmittedBallot.SubmittedBallotHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Create a SubmittedBallot
        /// </summary>
        /// <param name="ciphertext"></param>
        /// <param name="state"></param>
        public SubmittedBallot(CiphertextBallot ciphertext, BallotBoxState state)
        {
            var status = NativeInterface.SubmittedBallot.From(
                ciphertext.Handle, state, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Creates a <see cref="SubmittedBallot">SubmittedBallot</see> object from a <see href="https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1">[RFC-8259]</see> UTF-8 encoded JSON string
        /// </summary>
        /// <param name="json">A UTF-8 Encoded JSON data string</param>
        public SubmittedBallot(string json)
        {
            var status = NativeInterface.SubmittedBallot.FromJson(json, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a SubmittedBallot
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        public unsafe SubmittedBallot(byte[] data, BinarySerializationEncoding encoding)
        {
            fixed (byte* pointer = data)
            {
                var status = encoding == BinarySerializationEncoding.BSON
                    ? NativeInterface.SubmittedBallot.FromBson(pointer, (ulong)data.Length, out Handle)
                    : NativeInterface.SubmittedBallot.FromMsgPack(pointer, (ulong)data.Length, out Handle);
                status.ThrowIfError();
            }
        }

        /// <summary>
        /// Get a contest at a specific index
        /// </summary>
        public CiphertextBallotContest GetContestAt(ulong index)
        {
            var status = NativeInterface.SubmittedBallot.GetContestAtIndex(
                Handle,
                index,
                out var value);
            status.ThrowIfError();
            return new CiphertextBallotContest(value);
        }

        /// <summary>
        /// Given an encrypted Ballot, validates the encryption state
        /// against a specific ballot seed and public key
        /// by verifying the states of this ballot's members (BallotContest's and BallotSelection's).
        /// Calling this function expects that the object is in a well-formed encrypted state
        /// with the `contests` populated with valid encrypted ballot selections,
        /// and the ElementModQ `manifest_hash` also populated.
        /// Specifically, the seed in this context is the hash of the Election Manifest,
        /// or whatever `ElementModQ` was used to populate the `manifest_hash` field.
        /// </summary>
        public bool IsValidEncryption(
            ElementModQ manifestHash, ElementModP elGamalPublicKey, ElementModQ cryptoExtendedBaseHash)
        {
            return NativeInterface.SubmittedBallot.IsValidEncryption(
                Handle, manifestHash.Handle, elGamalPublicKey.Handle, cryptoExtendedBaseHash.Handle);
        }

        /// <Summary>
        /// Export the ballot representation as JSON
        /// </Summary>
        public string ToJson()
        {
            var status = NativeInterface.SubmittedBallot.ToJson(
                Handle, out IntPtr pointer, out _);
            status.ThrowIfError();
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToBson()
        {

            var status = NativeInterface.SubmittedBallot.ToBson(
                Handle, out IntPtr data, out ulong size);
            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("SubmittedBallot Error ToBson: size is too big");
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

            var status = NativeInterface.SubmittedBallot.ToMsgPack(
                Handle, out IntPtr data, out ulong size);

            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("SubmittedBallot Error ToMsgPack: size is too big");
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

    }
}
