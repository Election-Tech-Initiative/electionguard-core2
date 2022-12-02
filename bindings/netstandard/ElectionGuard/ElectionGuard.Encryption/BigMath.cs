using System.Reflection;
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

        public static ElementModP GPowP(ElementModQ elementModQ)
        {
            var status = External.QPowModP(Constants.TWO_MOD_P.Handle, elementModQ.Handle,
                out NativeInterface.ElementModP.ElementModPHandle value);
            status.ThrowIfError();
            return new ElementModP(value);
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
        }
    }
}
