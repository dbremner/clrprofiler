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
		    internal TreeNode.NodeType NodeType { get; }
		    internal int Id { get; }
		    internal string Name { get; }

		    public LineItem(int id, string name, TreeNode.NodeType nodeType)
		    {
                Id = id;
                Name = name;
                NodeType = nodeType;
		    }

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
