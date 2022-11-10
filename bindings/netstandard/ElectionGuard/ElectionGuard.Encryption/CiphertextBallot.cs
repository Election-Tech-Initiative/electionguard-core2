using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// A CiphertextBallot represents a voters encrypted selections for a given ballot and ballot style.
    ///
    /// When a ballot is in it's complete, encrypted state, the `nonce` is the seed nonce
    /// from which all other nonces can be derived to encrypt the ballot.  Along with the `nonce`
    /// fields on `BallotContest` and `BallotSelection`, this value is sensitive.
    ///
    /// Don't make this directly. Use `make_ciphertext_ballot` instead.
    /// </summary>
    public class CiphertextBallot : DisposableBase
    {
        internal unsafe NativeInterface.CiphertextBallot.CiphertextBallotHandle Handle;

        /// <summary>
        /// The unique ballot id that is meaningful to the consuming application.
        /// </summary>
        public unsafe string ObjectId
        {
            get
            {
                var status = NativeInterface.CiphertextBallot.GetObjectId(
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
        public unsafe string StyleId
        {
            get
            {
                var status = NativeInterface.CiphertextBallot.GetStyleId(
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
        public unsafe ElementModQ ManifestHash
        {
            get
            {
                var status = NativeInterface.CiphertextBallot.GetManifestHash(
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
        public unsafe ElementModQ BallotCodeSeed
        {
            get
            {
                var status = NativeInterface.CiphertextBallot.GetBallotCodeSeed(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// Get the size of the contests collection
        /// </summary>
        public unsafe ulong ContestsSize
        {
            get
            {
                var size = NativeInterface.CiphertextBallot.GetContestsSize(Handle);
                return size;
            }
        }

        /// <summary>
        /// The unique ballot code for this ballot that is derived from
        /// the ballot seed, the timestamp, and the hash of the encrypted values
        /// </summary>
        public unsafe ElementModQ BallotCode
        {
            get
            {
                var status = NativeInterface.CiphertextBallot.GetBallotCode(
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
        public unsafe ulong Timestamp
        {
            get
            {
                var status = NativeInterface.CiphertextBallot.GetTimestamp(
                    Handle, out ulong value);
                status.ThrowIfError();
                return value;
            }
        }

        /// <summary>
        /// The nonce value used to encrypt all values in the ballot
        /// </summary>
        public unsafe ElementModQ Nonce
        {
            get
            {
                var status = NativeInterface.CiphertextBallot.GetNonce(
                    Handle, out NativeInterface.ElementModQ.ElementModQHandle value);
                status.ThrowIfError();
                return new ElementModQ(value);
            }
        }

        /// <summary>
        /// Create a CiphertextBallot
        /// </summary>
        /// <param name="json"></param>
        public unsafe CiphertextBallot(string json)
        {
            var status = NativeInterface.CiphertextBallot.FromJson(json, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a CiphertextBallot
        /// </summary>
        /// <param name="data"></param>
        /// <param name="encoding"></param>
        public unsafe CiphertextBallot(byte[] data, BinarySerializationEncoding encoding)
        {
            fixed (byte* pointer = data)
            {
                var status = encoding == BinarySerializationEncoding.BSON
                    ? NativeInterface.CiphertextBallot.FromBson(pointer, (ulong)data.Length, out Handle)
                    : NativeInterface.CiphertextBallot.FromMsgPack(pointer, (ulong)data.Length, out Handle);
                status.ThrowIfError();
            }
        }

        internal unsafe CiphertextBallot(NativeInterface.CiphertextBallot.CiphertextBallotHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Get a contest at a specific index
        /// </summary>
        public unsafe CiphertextBallotContest GetContestAt(ulong index)
        {
            var status = NativeInterface.CiphertextBallot.GetContestAtIndex(
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
        public unsafe bool IsValidEncryption(
            ElementModQ manifestHash, ElementModP elGamalPublicKey, ElementModQ cryptoExtendedBaseHash)
        {
            return NativeInterface.CiphertextBallot.IsValidEncryption(
                Handle, manifestHash.Handle, elGamalPublicKey.Handle, cryptoExtendedBaseHash.Handle);
        }


        /// <Summary>
        /// Export the ballot representation as JSON
        /// </Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public unsafe string ToJson(bool withNonces = false)
        {
            var status = withNonces
                ? NativeInterface.CiphertextBallot.ToJsonWithNonces(
                    Handle, out IntPtr pointer, out _)
                : NativeInterface.CiphertextBallot.ToJson(
                    Handle, out pointer, out _);
            status.ThrowIfError();
            var json = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return json;
        }

        /// <Summary>
        /// Export the ballot representation as BSON
        /// </Summary>
        public unsafe byte[] ToBson(bool withNonces = false)
        {

            var status = withNonces
                ? NativeInterface.CiphertextBallot.ToBsonWithNonces(
                    Handle, out IntPtr data, out ulong size)
                : NativeInterface.CiphertextBallot.ToBson(
                    Handle, out data, out size);

            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("CiphertextBallot Error ToBson: size is too big");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.CompactCiphertextBallot.MsgPackFree(data);
            return byteArray;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public unsafe byte[] ToMsgPack(bool withNonces = false)
        {

            var status = withNonces
                ? NativeInterface.CiphertextBallot.ToMsgPack(
                    Handle, out IntPtr data, out ulong size)
                : NativeInterface.CiphertextBallot.ToMsgPack(
                    Handle, out data, out size);
            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("CiphertextBallot Error ToMsgPack: size is too big");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.CompactCiphertextBallot.MsgPackFree(data);
            return byteArray;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override unsafe void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null) return;
            Handle.Dispose();
            Handle = null;
        }
    }
}