using System;
using System.Data;
using System.Collections;
using JetBrains.Annotations;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for DiffDataNode.
	/// </summary>
	internal class DiffDataNode : TreeNodeBase
	{
		internal enum NodeType {Call = 0, Allocation, AssemblyLoad};

		//internal int parenttreeIdx = -1;
		//internal int treeIdx = -1;
		internal int parentId = -1;
		internal int nodeId = -1;
		internal string parentname = "";
		internal readonly string name;
		internal string mapname = "";
		internal long prevIncl = 0;
		internal long currIncl = 0;
		internal long diffIncl = 0;
		internal int prevFunId = -1;
		internal int currFunId = -1;
		internal long prevCalls = 0;
		internal long currCalls = 0;
		internal long diffCalls = 0;
			
		internal TreeNode currTreenode = null;
		internal TreeNode prevTreenode = null;
		internal bool marked = false;

		internal NodeType nodetype = 0;
	    [NotNull] internal readonly DiffStatistics data;
		internal bool highlighted;
		//internal ArrayList kids;
		
		
		public DiffDataNode([NotNull] String name)
		{
			this.name = name;
			int at = name.IndexOf('(');
			if(at > 0)
			{
				mapname = name.Substring(0, at);
			}
			allkids = new ArrayList();
			data = new DiffStatistics(this);
		}

	    public DiffDataNode([NotNull] String name, NodeType nodeType)
            : this(name)
	    {
            nodetype = nodeType;
	    }
	}
}
