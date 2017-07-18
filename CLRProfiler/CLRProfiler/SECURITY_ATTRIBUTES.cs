using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CLRProfiler
{
    struct SECURITY_ATTRIBUTES
    {
#pragma warning disable 414
        public int nLength;
#pragma warning restore 414
        public IntPtr lpSecurityDescriptor;
#pragma warning disable 414
        [MarshalAs(UnmanagedType.Bool)]
        public bool bInheritHandle;
#pragma warning restore 414
    }
}