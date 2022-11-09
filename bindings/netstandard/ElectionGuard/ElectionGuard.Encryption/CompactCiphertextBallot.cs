using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// A CompactCiphertextBallot is a CompactPlaintextBallot that includes the encryption parameters
    /// to properly re-encrypt the same ballot.
    ///
    /// This class is space optimized to serve specific use cases where an encrypted ballot is used
    /// to verify that plaintext selections have not been tampered with.
    ///
    /// Don't make this directly. Use `make` instead.
    /// </summary>
    public class CompactCiphertextBallot : DisposableBase
    {
        internal unsafe NativeInterface.CompactCiphertextBallot.CompactCiphertextBallotHandle Handle;

        /// <Summary>
        /// A unique Ballot ID that is relevant to the external system and must be unique
        /// within the dataset of the election.
        /// </Summary>
        public unsafe string ObjectId
        {
            get
            {
                var status = NativeInterface.CompactCiphertextBallot.GetObjectId(
                    Handle, out IntPtr value);
                status.ThrowIfError();
                var data = Marshal.PtrToStringAnsi(value);
                NativeInterface.Memory.FreeIntPtr(value);
                return data;
            }
        }

        /// <summary>
        /// Create a CompactCiphertextBallot
        /// </summary>
        /// <param name="data"></param>
        public unsafe CompactCiphertextBallot(byte[] data)
        {
            fixed (byte* pointer = data)
            {
                var status = NativeInterface.CompactCiphertextBallot.FromMsgPack(pointer, (ulong)data.Length, out Handle);
                status.ThrowIfError();
            }
        }

        internal unsafe CompactCiphertextBallot(NativeInterface.CompactCiphertextBallot.CompactCiphertextBallotHandle handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public unsafe byte[] ToMsgPack()
        {

            var status = NativeInterface.CompactCiphertextBallot.ToMsgPack(
                Handle, out IntPtr data, out ulong size);
            status.ThrowIfError();

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("CompactCiphertextBallot Error ToMsgPack: size is too big");
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