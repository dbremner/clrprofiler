// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using JetBrains.Annotations;

namespace CLRProfiler
{
    internal sealed partial class FunctionFind : System.Windows.Forms.Form
	{
	    private sealed class LineItem 
		{
		    internal TreeNode.NodeType NodeType { get; }
		    internal int Id { get; }

		    [NotNull]
		    internal string Name { get; }

		    public LineItem(int id, [NotNull] string name, TreeNode.NodeType nodeType)
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
