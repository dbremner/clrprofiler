using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal class TreeNodeBase
    {
        internal int depth;
        internal ArrayList allkids;

        internal bool HasKids;
        internal bool IsExpanded;

        internal TreeNodeBase()
        {
            depth = 0;
            IsExpanded = false;
        }
    }
}