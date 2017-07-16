using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CLRProfiler
{
    [StructLayout(LayoutKind.Sequential)]
    public struct IELAUNCHURLINFO
    {
        public int cbSize;
        public readonly int dwCreationFlags;
        public readonly int dwLaunchOptionFlags;
    }
}