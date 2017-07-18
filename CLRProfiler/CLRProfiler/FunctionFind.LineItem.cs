// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace CLRProfiler
{
    internal partial class FunctionFind : System.Windows.Forms.Form
	{
        class LineItem 
		{
			internal TreeNode.NodeType nodeType;
			internal int id;
			internal string Name;

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
