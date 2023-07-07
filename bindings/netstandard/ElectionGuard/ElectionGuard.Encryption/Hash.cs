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
        /// <Summary>
        /// Get the prefix for the hash of an election manifest
        /// </Summary>
        public static string Prefix00
        {
            get
            {
                var ptr = External.GetPrefix00();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// Get the prefix for the hash of a guardian key proof
        /// </Summary>
        public static string Prefix01
        {
            get
            {
                var ptr = External.GetPrefix01();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// Get the prefix for the hash of a guardian key share encryption proof
        /// </Summary>
        public static string Prefix02
        {
            get
            {
                var ptr = External.GetPrefix02();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// Get the prefix for the hash of an election extended base hash
        /// </Summary>
        public static string Prefix03
        {
            get
            {
                var ptr = External.GetPrefix03();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// Get the prefix for the hash of a ballot selection encryption proof
        /// </Summary>
        public static string Prefix04
        {
            get
            {
                var ptr = External.GetPrefix04();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// Get the prefix for the hash of a ballot contest data secret key
        /// </Summary>
        public static string tPrefix05
        {
            get
            {
                var ptr = External.GetPrefix05();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <Summary>
        /// Get the prefix for the hash of a ballot selection decryption proof
        /// </Summary>
        public static string Prefix06
        {
            get
            {
                var ptr = External.GetPrefix06();
                return Marshal.PtrToStringAnsi(ptr);
            }
        }

        /// <summary>
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        public static ElementModQ HashElems(string first)
        {
            var status = External.HashElems(first,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        public static ElementModQ HashElems(ulong first)
        {
            var status = External.HashElems(first,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the values
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
        /// Hash together the values
        /// </summary>
        /// <param name="first">first value for the hash</param>
        /// <param name="second">second value for the hash</param>
        public static ElementModQ HashElems(ulong first, ulong second)
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
            // TODO: ISSUE #239 we should define a clear method for 
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



        internal static class External
        {
            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_00",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix00();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_01",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix01();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_02",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix02();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_03",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]

            public static extern IntPtr GetPrefix03();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_04",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix04();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_05",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix05();

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_get_prefix_06",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true, CharSet = CharSet.Unicode)]
            public static extern IntPtr GetPrefix06();

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
                EntryPoint = "eg_hash_elems_int_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                ulong a,
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
