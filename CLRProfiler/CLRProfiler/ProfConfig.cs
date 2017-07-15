using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CLRProfiler
{
    // IMPORTANT: ProfConfig structure has a counterpart native structure defined
    // in ProfilerCallback.h.  Both must always be in sync.

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
    public struct ProfConfig
    {
        public OmvUsage usage;
        public int bOldFormat;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szPath;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szFileName;
        public int bDynamic;
        public int bStack;
        public uint dwFramesToPrint;
        public uint dwSkipObjects;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
        public string szClassToMonitor;
        public uint dwInitialSetting;
        public uint dwDefaultTimeoutMs;
        public bool bWindowsStoreApp;
    }
}