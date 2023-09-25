using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace ElectionGuard
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ValidationResult
    {
        public bool Success { get; set; }
        public List<string> Error { get; set; }
    }
}
