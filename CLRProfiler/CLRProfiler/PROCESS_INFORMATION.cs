using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CLRProfiler
{
    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        [SuppressMessage("Microsoft.Security","CA2111:PointersShouldNotBeVisible", Justification="CLRProfiler.exe is a stand-alone tool, not a library.")]
        public IntPtr hProcess;

        [SuppressMessage("Microsoft.Security","CA2111:PointersShouldNotBeVisible", Justification="CLRProfiler.exe is a stand-alone tool, not a library.")]
        public IntPtr hThread;

        public int dwProcessId;
        public int dwThreadId;
    }
}