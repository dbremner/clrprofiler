using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal sealed class ViewState
    {
        internal SortingBehaviour sort;
        internal SortingBehaviour highlight;
        internal bool showCalls;
        internal bool showAllocs;
        internal bool showAssemblies;

        internal ViewState(SortingBehaviour in_sort, SortingBehaviour in_highlight)
        {
            sort = in_sort;
            highlight = in_highlight;

            showCalls = true;
            showAllocs = true;
            showAssemblies = true;
        }
    }
}