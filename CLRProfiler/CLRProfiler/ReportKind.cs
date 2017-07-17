using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal enum ReportKind
    {
        NoReport,
        AllocationReport,
        RelocationReport,
        SurvivorReport,
        SurvivorDifferenceReport,
        HeapDumpReport,
        LeakReport,
        FinalizerReport,
        CriticalFinalizerReport,
        CommentReport,
    }
}