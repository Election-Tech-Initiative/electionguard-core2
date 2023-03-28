using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    public static class IntPtrExtension
    {
        public static IntPtr Offset(this IntPtr ptr, long that)
        {
            return new IntPtr(ptr.ToInt64() + that);
        }

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
