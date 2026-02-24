using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    /// <summary>
    /// Extension class to add functionality to the IntPtr class in .NET
    /// </summary>
    public static class IntPtrExtension
    {
        /// <summary>
        /// Creates a new IntPtr that is offset from the original
        /// </summary>
        /// <param name="ptr">original IntPtr</param>
        /// <param name="that">Amount to shift by</param>
        /// <returns>New IntPtr to the new address</returns>
        public static IntPtr Offset(this IntPtr ptr, long that)
        {
            return new IntPtr(ptr.ToInt64() + that);
        }

        /// <summary>
        /// Converts the data at the IntPtr to a c# string class
        /// </summary>
        /// <param name="rawPtr">IntPtr to convert</param>
        /// <returns>string found at the IntPtr</returns>
        public static string PtrToStringUTF8(this IntPtr rawPtr)
        {
            if (rawPtr == IntPtr.Zero)
            {
                return null;
            }

            var bytes = new List<byte>();
            while (true)
            {
                var b = Marshal.ReadByte(rawPtr);
                if (b == 0)
                {
                    break;
                }

                bytes.Add(b);
                rawPtr = rawPtr.Offset(1);
            }

            return System.Text.Encoding.UTF8.GetString(bytes.ToArray());
        }

    }
}
