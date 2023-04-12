using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    using NaiveElementModP = NativeInterface.ElementModP.ElementModPHandle;

    /// <summary>
    /// An element of the larger `mod p` space, i.e., in [0, P), where P is a 4096-bit prime.
    /// </summary>
    public class ElementModP : DisposableBase, IEquatable<ElementModP>
    {
        /// <summary>
        /// Number of 64-bit ints that make up the 4096-bit prime
        /// </summary>
        public static readonly ulong MaxSize = 64;

        /// <Summary>
        /// Get the integer representation of the element
        /// </Summary>
        public long[] Data
        {
            get => GetNative();
            set => NewNative(value);
        }

        internal NaiveElementModP Handle;

        /// <summary>
        /// Creates a `ElementModP` object
        /// </summary>
        /// <param name="newData">the data used to initialize the `ElementModP`</param>
        public ElementModP(ulong[] newData)
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
        /// Creates a `ElementModP` object
        /// </summary>
        /// <param name="newData">the data used to initialize the `ElementModP`</param>
        public ElementModP(byte[] newData)
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
        /// Create a `ElementModP`
        /// </summary>
        /// <param name="singleData">integer representing the value of the initialized data</param>
        /// <param name="uncheckedInput">if data is checked or not</param>
        public ElementModP(ulong singleData, bool uncheckedInput = false)
        {
            var status = uncheckedInput ?
                NativeInterface.ElementModP.FromUint64Unchecked(singleData, out Handle)
                : NativeInterface.ElementModP.FromUint64(singleData, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a `ElementModP`
        /// </summary>
        public ElementModP(ElementModQ elementModQ)
        {
            var status = NativeInterface.ElementModQ.ToElementModP(elementModQ.Handle, out Handle);
            status.ThrowIfError();
        }

        /// <summary>
        /// Create a copy of an existing `ElementModP`
        /// </summary>
        /// <param name="src">Existing `ElementModP` to copy</param>
        public ElementModP(ElementModP src)
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


        internal ElementModP(NaiveElementModP handle)
        {
            Handle = handle;
        }

        public ElementModP() : this(0L)
        {
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

        public override string ToString()
        {
            return ToHex();
        }

        /// <Summary>
        /// exports a hex representation of the integer value in Big Endian format
        /// </Summary>
        public string ToHex()
        {
            var status = NativeInterface.ElementModP.ToHex(Handle, out var pointer);
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
            var status = NativeInterface.ElementModP.ToBytes(Handle, out IntPtr data, out ulong size);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"ToBytes Error Status: {status}");
            }

            var byteArray = new byte[(int)size];
            Marshal.Copy(data, byteArray, 0, (int)size);
            NativeInterface.Memory.DeleteIntPtr(data);
            return byteArray;
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

                var status = NativeInterface.ElementModP.New(pointer, out Handle);
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

                var status = NativeInterface.ElementModP.New(pointer, out Handle);
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

                var status = NativeInterface.ElementModP.NewBytes(pointer, size, out Handle);
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
                _ = NativeInterface.ElementModP.GetData(Handle, &element, out ulong size);
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
                    data[i] = (long)element[i];
                }
            }

            return data;
        }

        public static bool operator ==(ElementModP a, ElementModP b)
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

        public static bool operator !=(ElementModP a, ElementModP b)
        {
            return !(a == b);
        }

        /// <summary>
        /// Check to see if the object is equal to the current instance 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as ElementModP);
        }

        public bool Equals(ElementModP other)
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
        /// Check to see if the residue is valid
        /// </summary>
        public bool IsValidResidue()
        {
            var status = NativeInterface.ElementModP.IsValidResidue(Handle, out bool isValid);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"IsValidResidue Error Status: {status}");
            }
            return isValid;
        }

        /// <summary>
        /// Check if the ElementModP is in bounds
        /// </summary>
        public bool IsInBounds()
        {
            var status = NativeInterface.ElementModP.IsInBounds(Handle, out bool inBounds);
            if (status != Status.ELECTIONGUARD_STATUS_SUCCESS)
            {
                throw new ElectionGuardException($"IsInBounds Error Status: {status}");
            }
            return inBounds;
        }

        /// <summary>
        /// Multiply by an ElementModP value
        /// </summary>
        /// <param name="rhs">right hand side for the multiply</param>
        public void MultModP(ElementModP rhs)
        {
            var status = NativeInterface.ElementModP.MultModP(Handle, rhs.Handle,
                out NativeInterface.ElementModP.ElementModPHandle value);
            Handle.Dispose();
            status.ThrowIfError();
            Handle = value;
        }

        /// <summary>
        /// Multiply list of ElementModP values
        /// </summary>
        /// <param name="keys">list of keys the multiply</param>
        public void MultModP(IEnumerable<ElementModP> keys)
        {
            foreach (var key in keys)
            {
                MultModP(key);
            }
        }
    }
}
