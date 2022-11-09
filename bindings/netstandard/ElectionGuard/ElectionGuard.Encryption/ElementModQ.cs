using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// An element of the smaller `mod q` space, i.e., in [0, Q), where Q is a 256-bit prime.
    /// </summary>
    public class ElementModQ : DisposableBase
    {
        /// <summary>
        /// Number of 64-bit ints that make up the 256-bit prime
        /// </summary>
        public static readonly ulong MAX_SIZE = 4;

        /// <Summary>
        /// Get the integer representation of the element
        /// </Summary>
        public ulong[] Data { get { return GetNative(); } internal set { NewNative(value); } }
        internal unsafe NativeInterface.ElementModQ.ElementModQHandle Handle;

        /// <summary>
        /// Create a `ElementModQ`
        /// </summary>
        /// <param name="data">data used to initialize the `ElementModQ`</param>
        public ElementModQ(ulong[] data)
        {
            try
            {
                NewNative(data);
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException("construction error", ex);
            }
        }

        /// <summary>
        /// Create a `ElementModQ`
        /// </summary>
        /// <param name="hex">string representing the hex bytes of the initializationd data</param>
        /// <param name="uncheckedInput">if data is checked or not</param>
        public ElementModQ(string hex, bool uncheckedInput = false)
        {
            var status = uncheckedInput ?
                NativeInterface.ElementModQ.FromHexUnchecked(hex, out Handle)
                : NativeInterface.ElementModQ.FromHex(hex, out Handle);
            status.ThrowIfError();
        }

        unsafe internal ElementModQ(NativeInterface.ElementModQ.ElementModQHandle handle)
        {
            Handle = handle;
        }

        /// <Summary>
        /// exports a hex representation of the integer value in Big Endian format
        /// </Summary>
        public unsafe string ToHex()
        {
            var status = NativeInterface.ElementModQ.ToHex(Handle, out IntPtr pointer);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ToHex Error Status: {status}");
            }
            var value = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return value;
        }

#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected override unsafe void DisposeUnmanaged()
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member
        {
            base.DisposeUnmanaged();

            if (Handle == null || Handle.IsInvalid) return;
            Handle.Dispose();
            Handle = null;
        }

        private unsafe void NewNative(ulong[] data)
        {
            fixed (ulong* pointer = new ulong[MAX_SIZE])
            {
                for (ulong i = 0; i < MAX_SIZE; i++)
                {
                    pointer[i] = data[i];
                }

                var status = NativeInterface.ElementModQ.New(pointer, out Handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"createNative Error Status: {status}");
                }
            }
        }

        private unsafe ulong[] GetNative()
        {
            if (Handle == null)
            {
                return null;
            }

            var data = new ulong[MAX_SIZE];
            fixed (ulong* element = new ulong[MAX_SIZE])
            {
                var status = NativeInterface.ElementModQ.GetData(Handle, &element, out ulong size);
                if (size != MAX_SIZE)
                {
                    throw new ElectionGuardException($"wrong size, expected: {MAX_SIZE}, actual: {size}");
                }

                if (element == null)
                {
                    throw new ElectionGuardException("element is null");
                }

                for (ulong i = 0; i < MAX_SIZE; i++)
                {
                    data[i] = element[i];
                }
            }

            return data;
        }
    }
}