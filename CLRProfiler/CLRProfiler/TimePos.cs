using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal struct TimePos
    {
        internal readonly double time;
        internal readonly long pos;

        internal TimePos(double time, long pos)
        {
            this.time = time;
            this.pos = pos;
        }
    }
}