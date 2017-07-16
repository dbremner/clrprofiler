using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal class TreeNodeBase
    {
        internal int depth;
        [CanBeNull] internal ArrayList allkids;

        internal bool HasKids;
        internal bool IsExpanded;

        internal TreeNodeBase()
        {
            depth = 0;
            IsExpanded = false;
        }
    }
}