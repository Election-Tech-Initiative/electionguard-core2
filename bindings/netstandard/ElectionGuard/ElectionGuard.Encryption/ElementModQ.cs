using System;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// An element of the smaller `mod q` space, i.e., in [0, Q), where Q is a 256-bit prime.
    /// </summary>
    public class ElementModQ : DisposableBase, IEquatable<ElementModQ>
    {
        /// <summary>
        /// Number of 64-bit ints that make up the 256-bit prime
        /// </summary>
        public const ulong MaxSize = 4;

        /// <Summary>
        /// Get the integer representation of the element
        /// </Summary>
        public long[] Data
        {
            get => GetNative();
            set => NewNative(value);
        }

        internal NativeInterface.ElementModQ.ElementModQHandle Handle;

        /// <summary>
        /// Create a `ElementModQ`
        /// </summary>
        /// <param name="newData">data used to initialize the `ElementModQ`</param>
        public ElementModQ(ulong[] newData)
        {
            try
            {
                NewNative(newData);
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException("construction error", ex);
            }
        }

        /// <summary>
        /// Create a copy of an existing `ElementModQ`
        /// </summary>
        /// <param name="src">Existing `ElementModQ` to copy</param>
        public ElementModQ(ElementModQ src)
        {
            try
            {
                NewNative(src.Data);
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException("construction error", ex);
            }
        }

        /// <summary>
        /// Creates a `ElementModP` object
        /// </summary>
        /// <param name="newData">the data used to initialize the `ElementModP`</param>
        public ElementModQ(byte[] newData)
        {
            try
            {
                NewNative(newData, (ulong)newData.Length);
            }
            catch (Exception ex)
            {
                throw new ElectionGuardException("construction error", ex);
            }
        }


        /// <summary>
        /// Create a `ElementModQ`
        /// </summary>
        /// <param name="hex">string representing the hex bytes of the initialized data</param>
        /// <param name="uncheckedInput">if data is checked or not</param>
        public ElementModQ(string hex, bool uncheckedInput = false)
        {
            var status = uncheckedInput ?
                NativeInterface.ElementModQ.FromHexUnchecked(hex, out Handle)
                : NativeInterface.ElementModQ.FromHex(hex, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a `ElementModQ`
        /// </summary>
        /// <param name="data">integer representing the value of the initialized data</param>
        /// <param name="uncheckedInput">if data is checked or not</param>
        public ElementModQ(ulong newData, bool uncheckedInput = false)
        {
            var status = uncheckedInput ?
                NativeInterface.ElementModQ.FromUint64Unchecked(newData, out Handle)
                : NativeInterface.ElementModQ.FromUint64(newData, out Handle);
            status.ThrowIfError();
        }

        internal ElementModQ(NativeInterface.ElementModQ.ElementModQHandle handle)
        {
            Handle = handle;
        }
        public ElementModQ() : this(0L)
        {
        }

        public override string ToString()
        {
            return ToHex();
        }

        /// <Summary>
        /// exports a hex representation of the integer value in Big Endian format
        /// </Summary>
        public string ToHex()
        {
            var status = NativeInterface.ElementModQ.ToHex(Handle, out var pointer);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ToHex Error Status: {status}");
            }
            var value = Marshal.PtrToStringAnsi(pointer);
            _ = NativeInterface.Memory.FreeIntPtr(pointer);
            return value;
        }

        /// <Summary>
        /// exports an array of bytes representation of the integer value in Big Endian format
        /// </Summary>
        public byte[] ToBytes()
        {
            var status = NativeInterface.ElementModQ.ToBytes(Handle, out IntPtr data, out ulong size);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ToBytes Error Status: {status}");
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

        private unsafe void NewNative(long[] data)
        {
            Handle?.Dispose();

            fixed (ulong* pointer = new ulong[MaxSize])
            {
                for (ulong i = 0; i < MaxSize; i++)
                {
                    pointer[i] = (ulong)data[i];
                }

                var status = NativeInterface.ElementModQ.New(pointer, out Handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"createNative Error Status: {status}");
                }
            }
        }
        private unsafe void NewNative(ulong[] data)
        {
            Handle?.Dispose();

            fixed (ulong* pointer = new ulong[MaxSize])
            {
                for (ulong i = 0; i < MaxSize; i++)
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

        private unsafe void NewNative(byte[] data, ulong size)
        {
            Handle?.Dispose();

            fixed (byte* pointer = new byte[size])
            {
                for (ulong i = 0; i < size; i++)
                {
                    pointer[i] = data[i];
                }

                var status = NativeInterface.ElementModQ.NewBytes(pointer, size, out Handle);
                if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
                {
                    throw new ElectionGuardException($"createNative Error Status: {status}");
                }
            }
        }


        private unsafe long[] GetNative()
        {
            if (Handle == null)
            {
                return null;
            }

            var data = new long[MaxSize];
            fixed (ulong* element = new ulong[MaxSize])
            {
                _ = NativeInterface.ElementModQ.GetData(Handle, &element, out var size);
                if (size != MaxSize)
                {
                    throw new ElectionGuardException($"wrong size, expected: {MaxSize}, actual: {size}");
                }

                if (element == null)
                {
                    throw new ElectionGuardException("element is null");
                }

                for (ulong i = 0; i < MaxSize; i++)
                {
                    data[i] = (long)element[i];
                }
            }

            return data;
        }

        public static bool operator ==(ElementModQ a, ElementModQ b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null || b is null)
            {
                return false;
            }

            return a.ToHex().Equals(b.ToHex());
        }

        public static bool operator !=(ElementModQ a, ElementModQ b)
        {
            return !(a == b);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ElementModQ);
        }

        public bool Equals(ElementModQ other)
        {
            if (other is null)
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return ToHex().Equals(other.ToHex());
        }

        /// <summary>
        /// Generates a hashcode for the class
        /// </summary>
        /// <returns>the hashcode</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(ToHex());
            return hashCode.GetHashCode();
        }

        /// <summary>
        /// Check if the ElementModQ is in bounds
        /// </summary>
        public bool IsInBounds()
        {
            var status = NativeInterface.ElementModQ.IsInBounds(Handle, out var inBounds);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"IsInBounds Error Status: {status}");
            }
            return inBounds;
        }


    }
}
