using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal struct SECURITY_ATTRIBUTES
    {
#pragma warning disable 414
        [UsedImplicitly]
        public int nLength;
#pragma warning restore 414
        public IntPtr lpSecurityDescriptor;
#pragma warning disable 414
        [UsedImplicitly]
        [MarshalAs(UnmanagedType.Bool)]
        public bool bInheritHandle;
#pragma warning restore 414
    }
}