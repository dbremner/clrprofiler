/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  CallTreeForm and many small data-holder classes and structures
 *
 * Description: Call tree view interface and all internal logic
 */

using System.Collections;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal sealed partial class CallTreeForm : System.Windows.Forms.Form, IComparer, ITreeOwner
    {
        internal sealed class ThreadState
        {
            internal int[] prevStackTrace;
            internal int prevStackLen;
            internal int prevDepth;
            internal ArrayList stack;
            internal ArrayList queuedNodes;
            internal SortedList functions;
            [NotNull] internal TreeListView callTreeView;
        }
    }
}
