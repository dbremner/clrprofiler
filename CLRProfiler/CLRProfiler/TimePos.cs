using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal struct TimePos
    {
        internal double time;
        internal long pos;

        internal TimePos(double time, long pos)
        {
            this.time = time;
            this.pos = pos;
        }
    }
}