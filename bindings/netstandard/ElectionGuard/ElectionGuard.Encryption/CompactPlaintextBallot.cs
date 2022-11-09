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
    public class CompactPlaintextBallot : DisposableBase
    {
        internal unsafe NativeInterface.CompactPlaintextBallot.CompactPlaintextBallotHandle Handle;

        /// <summary>
        /// Create a CompactPlaintextBallot
        /// </summary>
        /// <param name="data"></param>
        public unsafe CompactPlaintextBallot(byte[] data)
        {
            fixed (byte* pointer = data)
            {
                var status = NativeInterface.CompactPlaintextBallot.FromMsgPack(pointer, (ulong)data.Length, out Handle);
                status.ThrowIfError();
            }
        }

        internal unsafe CompactPlaintextBallot(NativeInterface.CompactPlaintextBallot.CompactPlaintextBallotHandle handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// Export the ballot representation as MsgPack
        /// </Summary>
        public unsafe byte[] ToMsgPack()
        {

            var status = NativeInterface.CompactPlaintextBallot.ToMsgPack(
                Handle, out IntPtr data, out ulong size);

            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"CompactPlaintextBallot Error ToMsgPack: {status}");
            }

            if (size > int.MaxValue)
            {
                throw new ElectionGuardException("CompactPlaintextBallot Error ToMsgPack: size is too big");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.CompactPlaintextBallot.MsgPackFree(data);
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