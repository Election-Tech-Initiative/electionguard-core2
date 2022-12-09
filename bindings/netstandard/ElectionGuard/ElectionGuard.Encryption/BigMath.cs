using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Methods related to numbers larger than 64 bits
    /// </summary>
    public static class BigMath
    {
        /// <summary>
        /// Generate random number between 0 and Q.
        /// </summary>
        public static ElementModQ RandQ()
        {
            var status = External.RandQ(
                out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Raise the value G by the given exponent
        /// </summary>
        /// <param name="elementModQ">Value to use as an exponent</param>
        public static ElementModP GPowP(ElementModQ elementModQ)
        {
            var status = External.QPowModP(Constants.G.Handle, elementModQ.Handle,
                out NativeInterface.ElementModP.ElementModPHandle value);
            status.ThrowIfError();
            return new ElementModP(value);
        }

        /// <summary>
        /// Add two ElementModQ values
        /// </summary>
        /// <param name="lhs">First paramter to add</param>
        /// <param name="rhs">Second parameter to add</param>
        public static ElementModQ AddModQ(ElementModQ lhs, ElementModQ rhs)
        {
            var status = External.AddModQ(lhs.Handle, rhs.Handle,
                out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
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
        /// Add two ElementModP values
        /// </summary>
        /// <param name="lhs">First paramter to add</param>
        /// <param name="rhs">Second parameter to add</param>
        public static ElementModP AddModP(ElementModP lhs, ElementModP rhs)
        {
            var status = External.AddModP(lhs.Handle, rhs.Handle,
                out NativeInterface.ElementModP.ElementModPHandle value);
            status.ThrowIfError();
            return new ElementModP(value);
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
        /// Calculate the formula a+b*c mod Q
        /// </summary>
        /// <param name="a">Value to add</param>
        /// <param name="b">First parameter to multiply</param>
        /// <param name="c">Second parameter to multiply</param>
        public static ElementModQ APlusBMulCModQ(ElementModQ a, ElementModQ b, ElementModQ c)
        {
            var status = External.APlusBMulCModQ(a.Handle, b.Handle, c.Handle,
                out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Multiple two ElementModP values
        /// </summary>
        /// <param name="lhs">left hand side for the multiply</param>
        /// <param name="rhs">right hand side for the multiply</param>
        public static ElementModP MultModP(ElementModP lhs, ElementModP rhs)
        {
            var status = External.MultModP(lhs.Handle, rhs.Handle,
                out NativeInterface.ElementModP.ElementModPHandle value);
            status.ThrowIfError();
            return new ElementModP(value);
        }

        /// <summary>
        /// Raise a ElementModP value to an ElementModP exponent
        /// </summary>
        /// <param name="b">base value for the calculation</param>
        /// <param name="e">exponent to raise the base by</param>
        public static ElementModP PowModP(ElementModP b, ElementModP e)
        {
            var status = External.PowModP(b.Handle, e.Handle,
                out NativeInterface.ElementModP.ElementModPHandle value);
            status.ThrowIfError();
            return new ElementModP(value);
        }

        /// <summary>
        /// Raise a ElementModQ value to an ElementModQ exponent
        /// </summary>
        /// <param name="b">base value for the calculation</param>
        /// <param name="e">exponent to raise the base by</param>
        public static ElementModQ PowModQ(ElementModQ b, ElementModQ e)
        {
            var status = External.PowModQ(b.Handle, e.Handle,
                out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Hash together the ElementModP values
        /// </summary>
        /// <param name="publickey">first value for the hash</param>
        /// <param name="commitment">second value for the hash</param>
        public static ElementModQ HashElems(ElementModP publickey, ElementModP commitment)
        {
            var status = External.HashElems(publickey.Handle, commitment.Handle,
                out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        /// <summary>
        /// Multiple two ElementModQ values
        /// </summary>
        /// <param name="lhs">left hand side for the multiply</param>
        /// <param name="rhs">right hand side for the multiply</param>
        public static ElementModQ MultModQ(ElementModQ lhs, ElementModQ rhs)
        {
            var status = External.MultModQ(lhs.Handle, rhs.Handle,
                out NativeInterface.ElementModQ.ElementModQHandle value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        internal static class External
        {
            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_rand_q_new",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status RandQ(out NativeInterface.ElementModQ.ElementModQHandle handle);

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_element_mod_q_pow_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status QPowModP(
                NativeInterface.ElementModP.ElementModPHandle base1,
                NativeInterface.ElementModQ.ElementModQHandle exponent,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

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
                EntryPoint = "eg_element_mod_p_pow_mod_p",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status PowModP(
                NativeInterface.ElementModP.ElementModPHandle b,
                NativeInterface.ElementModP.ElementModPHandle e,
                out NativeInterface.ElementModP.ElementModPHandle handle
                );

            [DllImport(
                NativeInterface.DllName,
                EntryPoint = "eg_hash_elems",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status HashElems(
                NativeInterface.ElementModP.ElementModPHandle publickey,
                NativeInterface.ElementModP.ElementModPHandle commitment,
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
                EntryPoint = "eg_element_mod_q_pow_mod_q",
                CallingConvention = CallingConvention.Cdecl,
                SetLastError = true)]
            internal static extern Status PowModQ(
                NativeInterface.ElementModQ.ElementModQHandle b,
                NativeInterface.ElementModQ.ElementModQHandle e,
                out NativeInterface.ElementModQ.ElementModQHandle handle
                );

        }
    }
}
