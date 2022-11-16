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
    public partial class CiphertextBallot : DisposableBase
    {
        /// <summary>
        /// Create a CiphertextBallot
        /// </summary>
        /// <param name="json"></param>
        public CiphertextBallot(string json)
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

        internal CiphertextBallot(External.CiphertextBallotHandle handle)
        {
            Handle = handle;
        }

        /// <summary>
        /// Get a contest at a specific index
        /// </summary>
        public CiphertextBallotContest GetContestAt(ulong index)
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
        public bool IsValidEncryption(
            ElementModQ manifestHash, ElementModP elGamalPublicKey, ElementModQ cryptoExtendedBaseHash)
        {
            return NativeInterface.CiphertextBallot.IsValidEncryption(
                Handle, manifestHash.Handle, elGamalPublicKey.Handle, cryptoExtendedBaseHash.Handle);
        }


        /// <Summary>
        /// Export the ballot representation as JSON
        /// </Summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0059:Unnecessary assignment of a value", Justification = "<Pending>")]
        public string ToJson(bool withNonces = false)
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
        public byte[] ToBson(bool withNonces = false)
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
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public byte[] ToMsgPack(bool withNonces = false)
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
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
        }
    }
}