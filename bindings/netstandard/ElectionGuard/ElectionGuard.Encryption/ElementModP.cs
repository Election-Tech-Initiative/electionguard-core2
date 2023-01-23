using System;
using System.Collections.Generic;
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
        /// Creates a `ElementModP` object
        /// </summary>
        /// <param name="data">the data used to initialize the `ElementModP`</param>
        public ElementModP(byte[] data)
        {
            try
            {
                NewNative(data, (ulong)data.Length);
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
        private unsafe void NewNative(byte[] data, ulong size)
        {
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            if (!(obj is ElementModP other))
                return false;
            return this.ToHex() == other.ToHex();
        }

        /// <summary>
        /// Generates a hashcode for the class
        /// </summary>
        /// <returns>the hashcode</returns>
        public override int GetHashCode()
        {
            var hashCode = new HashCode();
            hashCode.Add(this.ToHex());
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
