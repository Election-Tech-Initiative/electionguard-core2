using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace ElectionGuard
{
    public static class IntPtrExtension
    {
        public static IntPtr Offset(this IntPtr ptr, Int64 that)
        {
            return new IntPtr(ptr.ToInt64() + (that));
        }

        public static string PtrToStringUTF8(this IntPtr rawPtr)
        {
            if (rawPtr == IntPtr.Zero)
            {
                return null;
            }

            List<Byte> bytes = new List<Byte>();
            while (true)
            {
                Byte b = Marshal.ReadByte(rawPtr);
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
