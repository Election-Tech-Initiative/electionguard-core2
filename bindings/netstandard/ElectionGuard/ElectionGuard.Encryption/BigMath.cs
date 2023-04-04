using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Methods related to numbers larger than 64 bits
    /// </summary>
    public static class BigMath
    {

        #region ElementModP Group Math Functions

        /// <summary>
        /// Add two ElementModP values
        /// </summary>
        /// <param name="lhs">First paramter to add</param>
        /// <param name="rhs">Second parameter to add</param>
        public static ElementModP AddModP(ElementModP lhs, ElementModP rhs)
        {
            var status = External.AddModP(lhs.Handle, rhs.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModP(value);
        }

        /// <summary>
        /// Add an ElementModP value and an integer
        /// </summary>
        /// <param name="lhs">First paramter to add</param>
        /// <param name="rhs">Second parameter to add</param>
        public static ElementModP AddModP(ElementModP lhs, ulong rhs)
        {
            return AddModP(lhs, new ElementModP(rhs, true));
        }

        /// <summary>
        /// Multiple two ElementModP values
        /// </summary>
        /// <param name="lhs">left hand side for the multiply</param>
        /// <param name="rhs">right hand side for the multiply</param>
        public static ElementModP MultModP(ElementModP lhs, ElementModP rhs)
        {
            var status = External.MultModP(lhs.Handle, rhs.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModP(value);
        }

        /// <summary>
        /// Divide two ElementModP values
        /// </summary>
        /// <param name="numerator">the numerator</param>
        /// <param name="denominator">the denominator</param>
        public static ElementModP DivModP(ElementModP numerator, ElementModP denominator)
        {
            var status = External.DivModP(numerator.Handle, denominator.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModP(value);
        }

        /// <summary>
        /// Raise a ElementModP value to an ElementModP exponent
        /// </summary>
        /// <param name="b">base value for the calculation</param>
        /// <param name="e">exponent to raise the base by</param>
        public static ElementModP PowModP(ElementModP b, ElementModP e)
        {
            var status = External.PowModP(b.Handle, e.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModP(value);
        }

        /// <summary>
        /// Raise a ElementModP value to an ElementModP exponent
        /// </summary>
        /// <param name="b">base value for the calculation</param>
        /// <param name="e">exponent to raise the base by</param>
        public static ElementModP PowModP(ElementModP b, ElementModQ e)
        {
            var status = External.QPowModP(b.Handle, e.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModP(value);
        }

        public static ElementModP PowModP(ElementModP b, ulong e)
        {
            return PowModP(b, new ElementModQ(e, true));
        }

        public static ElementModP PowModP(ElementModQ b, ulong e)
        {
            return PowModP(new ElementModP(b), new ElementModQ(e, true));
        }

        /// <summary>
        /// Raise a ElementModP value to an ElementModP exponent
        /// </summary>
        /// <param name="b">base value for the calculation</param>
        /// <param name="e">exponent to raise the base by</param>
        public static ElementModP PowModP(ulong b, ulong e)
        {
            var status = External.LongPowModP(b, e,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModP(value);
        }

        /// <summary>
        /// Raise the value G by the given exponent
        /// </summary>
        /// <param name="elementModQ">Value to use as an exponent</param>
        public static ElementModP GPowP(ElementModQ elementModQ)
        {
            var status = External.QPowModP(Constants.G.Handle, elementModQ.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModP(value);
        }

        #endregion

        #region ElementModQ Group Math Functions

        /// <summary>
        /// Add two ElementModQ values
        /// </summary>
        /// <param name="lhs">First paramter to add</param>
        /// <param name="rhs">Second parameter to add</param>
        public static ElementModQ AddModQ(ElementModQ lhs, ElementModQ rhs)
        {
            var status = External.AddModQ(lhs.Handle, rhs.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Add an ElementModQ value and an integer
        /// </summary>
        /// <param name="lhs">First paramter to add</param>
        /// <param name="rhs">Second parameter to add</param>
        public static ElementModQ AddModQ(ElementModQ lhs, ulong rhs)
        {
            return AddModQ(lhs, new ElementModQ(rhs, true));
        }

        /// <summary>
        /// Subtract two ElementModQ values
        /// </summary>
        /// <param name="lhs">First paramter to add</param>
        /// <param name="rhs">Second parameter to add</param>
        public static ElementModQ SubModQ(ElementModQ lhs, ElementModQ rhs)
        {
            var status = External.SubModQ(lhs.Handle, rhs.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Multiply two ElementModQ values
        /// </summary>
        /// <param name="lhs">left hand side for the multiply</param>
        /// <param name="rhs">right hand side for the multiply</param>
        public static ElementModQ MultModQ(ElementModQ lhs, ElementModQ rhs)
        {
            var status = External.MultModQ(lhs.Handle, rhs.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Divide two ElementModQ values
        /// </summary>
        /// <param name="numerator">the numerator</param>
        /// <param name="denominator">the denominator</param>
        public static ElementModQ DivModQ(ElementModQ numerator, ElementModQ denominator)
        {
            var status = External.DivModQ(numerator.Handle, denominator.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Raise a ElementModQ value to an ElementModQ exponent
        /// </summary>
        /// <param name="b">base value for the calculation</param>
        /// <param name="e">exponent to raise the base by</param>
        public static ElementModQ PowModQ(ElementModQ b, ElementModQ e)
        {
            var status = External.PowModQ(b.Handle, e.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Raise a ElementModQ value to a long exponent
        /// </summary>
        /// <param name="b">base value for the calculation</param>
        /// <param name="e">exponent to raise the base by</param>
        public static ElementModQ PowModQ(ElementModQ b, ulong e)
        {
            var status = External.LongPowModQ(b.Handle, e,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Calculate the formula a+b*c mod Q
        /// </summary>
        /// <param name="a">Value to add</param>
        /// <param name="b">First parameter to multiply</param>
        /// <param name="c">Second parameter to multiply</param>
        public static ElementModQ APlusBMulCModQ(ElementModQ a, ElementModQ b, ElementModQ c)
        {
            var status = External.APlusBMulCModQ(a.Handle, b.Handle, c.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Generate random number between 0 and Q.
        /// </summary>
        public static ElementModQ RandQ()
        {
            var status = External.RandQ(
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        #endregion

        #region Group Hash Functions

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="publickey">first value for the hash</param>
        /// <param name="commitment">second value for the hash</param>
        public static ElementModQ HashElems(ElementModP publickey, ElementModP commitment)
        {
            var status = External.HashElems(publickey.Handle, commitment.Handle,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="id">first value for the hash</param>
        /// <param name="sequence">second value for the hash</param>
        public static ElementModQ HashElems(string id, ulong sequence)
        {
            var status = External.HashElems(id, sequence,
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
                data[i].Dispose();
            }

            var status = External.HashElems(dataPointers, (ulong)data.Count,
                out var value);
            status.ThrowIfError();
            return value.IsInvalid ? null : new ElementModQ(value);
        }

        #endregion

        internal static class External
        {
            #region ElementModP Group Math Functions

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_p_add_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status AddModP(
                NativeInterface.ElementModP.ElementModPHandle lhs,
                NativeInterface.ElementModP.ElementModPHandle rhs,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_p_mult_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status MultModP(
                NativeInterface.ElementModP.ElementModPHandle lhs,
                NativeInterface.ElementModP.ElementModPHandle rhs,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_p_div_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status DivModP(
                NativeInterface.ElementModP.ElementModPHandle numerator,
                NativeInterface.ElementModP.ElementModPHandle denominator,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_p_pow_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status PowModP(
                NativeInterface.ElementModP.ElementModPHandle @base,
                NativeInterface.ElementModP.ElementModPHandle exponent,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_long_pow_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status LongPowModP(
                ulong @base,
                ulong exponent,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_pow_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status QPowModP(
                NativeInterface.ElementModP.ElementModPHandle @base,
                NativeInterface.ElementModQ.ElementModQHandle exponent,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

            #endregion

            #region ElementModQ Group Math Functions

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_add_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status AddModQ(
                NativeInterface.ElementModQ.ElementModQHandle lhs,
                NativeInterface.ElementModQ.ElementModQHandle rhs,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_sub_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status SubModQ(
                NativeInterface.ElementModQ.ElementModQHandle lhs,
                NativeInterface.ElementModQ.ElementModQHandle rhs,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_mult_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status MultModQ(
                NativeInterface.ElementModQ.ElementModQHandle lhs,
                NativeInterface.ElementModQ.ElementModQHandle rhs,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_div_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status DivModQ(
                NativeInterface.ElementModQ.ElementModQHandle numerator,
                NativeInterface.ElementModQ.ElementModQHandle denominator,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_pow_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status PowModQ(
                NativeInterface.ElementModQ.ElementModQHandle @base,
                NativeInterface.ElementModQ.ElementModQHandle exponent,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_long_pow_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status LongPowModQ(
                NativeInterface.ElementModQ.ElementModQHandle @base,
                ulong exponent,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_a_plus_b_mul_c_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status APlusBMulCModQ(
                NativeInterface.ElementModQ.ElementModQHandle a,
                NativeInterface.ElementModQ.ElementModQHandle b,
                NativeInterface.ElementModQ.ElementModQHandle c,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_rand_q_new",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status RandQ(out NativeInterface.ElementModQ.ElementModQHandle handle);

            #endregion

            #region Group Hash Functions

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_modp_modp",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModP.ElementModPHandle publickey,
                NativeInterface.ElementModP.ElementModPHandle commitment,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_string_int",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                string publickey,
                ulong commitment,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems_array",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                [MarshalAs(UnmanagedType.LPArray)] IntPtr[] inData,
                ulong inDataSize,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

            #endregion
        }
    }
}
