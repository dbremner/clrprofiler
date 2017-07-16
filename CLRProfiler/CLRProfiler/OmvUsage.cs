using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
// ReSharper disable UnusedMember.Global

namespace CLRProfiler
{
    [Flags]
    public enum OmvUsage : int
    {
        OmvUsageNone = 0,
        OmvUsageObjects = 1,
        OmvUsageTrace = 2,
        OmvUsageBoth = 3
    }
}