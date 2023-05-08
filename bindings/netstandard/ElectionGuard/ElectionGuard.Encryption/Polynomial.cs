using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// A polynomial interpolation function
    /// </summary>
    public class Polynomial
    {
        /// <summary>
        /// Compute the lagrange polynomial interpolation coefficient for a specific coordinate against N degrees.
        /// <param name="coordinate"> the coordinate to plot, uisually a Guardian's Sequence Order</param>
        /// <param name="degrees"> the degrees across which to plot, usually the collection of available Guardians' Sequence Orders</param>
        /// </summary>
        public static ElementModQ Interpolate(ulong coordinate, List<ulong> degrees)
        {
            return Interpolate(
                new ElementModQ(coordinate),
                degrees.ConvertAll(x => new ElementModQ(x)));
        }

        /// <summary>
        /// Compute the lagrange polynomial interpolation coefficient for a specific coordinate against N degrees.
        /// <param name="coordinate"> the coordinate to plot, uisually a Guardian's Sequence Order</param>
        /// <param name="degrees"> the degrees across which to plot, usually the collection of available Guardians' Sequence Orders</param>
        /// </summary>
        public static ElementModQ Interpolate(ElementModQ coordinate, List<ElementModQ> degrees)
        {
            var dataPointers = new IntPtr[degrees.Count];
            for (var i = 0; i < degrees.Count; i++)
            {
                dataPointers[i] = degrees[i].Handle.Ptr;
            }
            var status = External.Interpolate(coordinate.Handle, dataPointers, (ulong)degrees.Count, out var value);
            status.ThrowIfError();
            return new ElementModQ(value);
        }

        #region Extern

        internal static unsafe class External
        {
            [DllImport(NativeInterface.DllName, EntryPoint = "eg_polynomial_interpolate",
            CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
            public static extern Status Interpolate(
                NativeInterface.ElementModQ.ElementModQHandle coordinate,
                IntPtr[] degrees,
                ulong degreesLength,
                out NativeInterface.ElementModQ.ElementModQHandle outHandle);
        }

        #endregion
    }
}
