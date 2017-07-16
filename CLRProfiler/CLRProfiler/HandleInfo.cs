using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal class HandleInfo
    {
        internal HandleInfo(int allocThreadId, ulong handleId, ulong initialObjectId, int allocTickIndex, int allocStacktraceId)
        {
            this.allocThreadId = allocThreadId;
            this.handleId = handleId;
            this.initialObjectId = initialObjectId;
            this.allocTickIndex = allocTickIndex;
            this.allocStacktraceId = allocStacktraceId;
        }

        internal int allocThreadId;
        internal ulong handleId;
        internal ulong initialObjectId;
        internal int allocTickIndex;
        internal readonly int allocStacktraceId;
    };
}