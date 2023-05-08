using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using ElectionGuard.Base;

namespace ElectionGuard
{

    /// <summary>
    /// Class for wrapping hashing methods
    /// </summary>
    public static class Hash
    {

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="sequence">second value for the hash</param>
        public static ElementModQ HashElems(string first)
        {
            var status = External.HashElems(first,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="id">first value for the hash</param>
        /// <param name="first">second value for the hash</param>
        public static ElementModQ HashElems(ulong first)
        {
            var status = External.HashElems(first,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(string first, ulong second)
        {
            var status = External.HashElems(first, second,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        /// <param name="third">third value for the hash</param>
        public static ElementModQ HashElems(string first, ulong second, ElementModP third)
        {
            var status = External.HashElems(first, second, third.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        /// <param name="third">third value for the hash</param>
        public static ElementModQ HashElems(string first, ulong second, ElementModQ third)
        {
            var status = External.HashElems(first, second, third.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModP first, ElementModP second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModP first, ElementModQ second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModQ first, ElementModQ second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ElementModQ first, ElementModP second)
        {
            var status = External.HashElems(first.Handle, second.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="data">List of ElementModP to hash together</param>
        public static ElementModQ HashElems(List<ElementModP> data)
        {
            var dataPointers = new IntPtr[data.Count];
            for (var i = 0; i < data.Count; i++)
            {
                dataPointers[i] = data[i].Handle.Ptr;
                // TODO: Do we really want to dispose here?
                data[i].Dispose();
            }

            var status = External.HashElems(dataPointers, (ulong)data.Count,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ICryptoHashableType values
        /// </summary>
        public static ElementModQ HashElems(string prefix, params ICryptoHashableType[] data)
        {
            // TODO: we should define a clear method for 
            // passing hashable elements back and forth
            // between C# and C++ so we don't have to do this
            var hashed = HashElems(prefix);
            foreach (var item in data)
            {
                if (item is CryptoHashableBase)
                {
                    if (item is ElementModP p)
                    {
                        using (var combined = HashElems(hashed, p))
                        {
                            hashed.Reassign(combined);
                        }

                    }
                    else if (item is ElementModQ q)
                    {

                        using (var combined = HashElems(hashed, q))
                        {
                            hashed.Reassign(combined);
                        }
                    }
                }
                else
                {
                    throw new ArgumentException("data must be of type CryptoHashableBase");
                }
            }
            return hashed;
        }

        // TODO: enforce polymorphism
        // public static ElementModQ HashElems(string prefix, params ICryptoHashableType[] data)
        // {
        //     var dataPointers = new IntPtr[data.Length];
        //     for (var i = 0; i < data.Length; i++)
        //     {
        //         if (data[i] is CryptoHashableBase)
        //         {
        //             dataPointers[i] = (data[i] as CryptoHashableBase).Ptr;
        //         }
        //         else
        //         {
        //             throw new ArgumentException("data must be of type CryptoHashableBase");
        //         }

        //     }

        //     var status = External.HashElems(dataPointers, (ulong)data.Length,
        //         out var value);
        //     status.ThrowIfError();
        //     return value.IsInvalid ? null : new ElementModQ(value);
        // }

        internal static class External
        {
            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_strings",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.LPStr)] string[] a,
                ulong length,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                ulong a,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                ulong b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string_int_modp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                ulong b, NativeInterface.ElementModP.ElementModPHandle c,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string_int_modq",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPStr)] string a,
                ulong b, NativeInterface.ElementModQ.ElementModQHandle c,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modp_modp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModP.ElementModPHandle a,
                NativeInterface.ElementModP.ElementModPHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modp_modq",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModP.ElementModPHandle a,
                NativeInterface.ElementModQ.ElementModQHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modq_modq",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModQ.ElementModQHandle a,
                NativeInterface.ElementModQ.ElementModQHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modq_modp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModQ.ElementModQHandle a,
                NativeInterface.ElementModP.ElementModPHandle b,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_array",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                // TODO: type safety
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] inData,
                ulong inDataSize,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );
        }
    }
}
