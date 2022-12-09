using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    using NaiveElementModP = NativeInterface.ElementModP.ElementModPHandle;

    /// <summary>
    /// An element of the larger `mod p` space, i.e., in [0, P), where P is a 4096-bit prime.
    /// </summary>
    public class ElementModP : DisposableBase
    {
        /// <summary>
        /// Number of 64-bit ints that make up the 4096-bit prime
        /// </summary>
        public static readonly ulong MaxSize = 64;

        /// <Summary>
        /// Get the integer representation of the element
        /// </Summary>
        public ulong[] Data
        {
            get => GetNative();
            internal set => NewNative(value);
        }

        internal NaiveElementModP Handle;

        /// <summary>
        /// Creates a `ElementModP` object
        /// </summary>
        /// <param name="data">the data used to initialize the `ElementModP`</param>
        public ElementModP(ulong[] data)
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
        /// Create a `ElementModP`
        /// </summary>
        /// <param name="data">integer representing the value of the initialized data</param>
        /// <param name="uncheckedInput">if data is checked or not</param>
        public ElementModP(ulong data, bool uncheckedInput = false)
        {
            var status = uncheckedInput ?
                NativeInterface.ElementModP.FromUint64Unchecked(data, out Handle)
                : NativeInterface.ElementModP.FromUint64(data, out Handle);
            status.ThrowIfError();
        }

        internal ElementModP(NaiveElementModP handle)
        {
            Handle = handle;
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
        /// exports a hex representation of the integer value in Big Endian format
        /// </Summary>
        public string ToHex()
        {
            var status = NativeInterface.ElementModP.ToHex(Handle, out IntPtr pointer);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ToHex Error Status: {status}");
            }
            var value = Marshal.PtrToStringAnsi(pointer);
            NativeInterface.Memory.FreeIntPtr(pointer);
            return value;
        }

        private unsafe void NewNative(ulong[] data)
        {
            fixed (ulong* pointer = new ulong[MaxSize])
            {
                for (ulong i = 0; i < MaxSize; i++)
                {
                    pointer[i] = data[i];
                }

                var status = NativeInterface.ElementModP.New(pointer, out Handle);
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

            var data = new ulong[MaxSize];
            fixed (ulong* element = new ulong[MaxSize])
            {
                NativeInterface.ElementModP.GetData(Handle, &element, out ulong size);
                if (size != MaxSize)
                {
                    throw new ElectionGuardException($"wrong size, expected: {MaxSize} actual: {size}");
                }

                if (element == null)
                {
                    throw new ElectionGuardException("element is null");
                }

                for (ulong i = 0; i < MaxSize; i++)
                {
                    data[i] = element[i];
                }
            }

            return data;
        }
    }
}
