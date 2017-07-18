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

namespace CLRProfiler
{
    internal partial class CallTreeForm : System.Windows.Forms.Form, IComparer, ITreeOwner
    {
        internal struct FnViewFilter
        {
            internal TreeNode.NodeType nodetype;
            internal int functionId;

            internal FnViewFilter( TreeNode.NodeType Nodetype, int FunctionId)
            {
                nodetype = Nodetype;
                functionId = FunctionId;
            }
        }
    }
}
