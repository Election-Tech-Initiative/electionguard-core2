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
        /// Get the BallotBoxState
        /// </summary>
        public BallotBoxState State => NativeInterface.CiphertextBallot.GetState(Handle);

        /// <summary>
        /// Creates an <see cref="CiphertextBallot">CiphertextBallot</see> object from a <see href="https://www.rfc-editor.org/rfc/rfc8259.html#section-8.1">[RFC-8259]</see> UTF-8 encoded JSON string
        /// </summary>
        /// <param name="json">A UTF-8 Encoded JSON data string</param>
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
        /// A helper function to mark the ballot as cast and remove sensitive values like the nonce.
        /// </summary>
        public void Cast()
        {
            var status = NativeInterface.CiphertextBallot.Cast(Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// A helper function to mark the ballot as spoiled and remove sensitive values like the nonce.
        /// </summary>
        public void Spoil()
        {
            var status = NativeInterface.CiphertextBallot.Spoil(Handle);
            status.ThrowIfError();
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