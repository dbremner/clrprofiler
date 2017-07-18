// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;

using System.IO;
using System.Text;
using System.Diagnostics;
using Microsoft.Win32;


namespace CLRProfiler
{
	
	/// <summary>
	/// Summary description for DiffCallTreeForm.
	/// </summary>
	internal partial class DiffCallTreeForm : System.Windows.Forms.Form, IComparer, IDiffTreeOwner
	{	
		private readonly AllocationDiff	_allocDiff;
		private ViewState viewState;
		private readonly Font defaultFont;
		private readonly TreeNodeBase Root;
		internal CLRProfiler.DiffTreeListView diffCallTreeView;
		private Rectangle formRect = new Rectangle( -1, -1, -1, -1 );
		
		public DiffCallTreeForm(TreeNodeBase root, AllocationDiff allocDiff)
		{
			this.Root = root;
			this._allocDiff = allocDiff;

		    Controls.Clear();
		    controlCollection?.Controls.Clear();
		    InitializeComponent();
		    defaultFont = new Font(new FontFamily("Tahoma"), 8);
			
		    var treeView = new DiffTreeListView(this);
		    treeView.Dock = DockStyle.Fill;
		    treeView.Font = defaultFont;

		    var sort = new SortingBehaviour();
		    sort.SortingOrder = -1;
		    sort.CounterId = -1;
		    var highlight = new SortingBehaviour();
            highlight.SortingOrder = -1;
		    highlight.CounterId = 2;

		    /* add columns */
		    treeView.AddColumn(new ColumnInformation(-1, "Function name", ColumnInformation.ColumnTypes.Tree), 250);
		    foreach(int counter in DiffStatistics.DefaultCounters)
		    {
		        AddColumn(treeView, counter);
		    }

		    treeView.ColumnClick += new EventHandler(SortOn);
			
		    treeView.TokenObject = new ViewState(sort, highlight);
		    treeView.Root = Root;
			
			
		    treeView.Size = new System.Drawing.Size(332, 108);
		    treeView.Visible = true;
		    controlCollection.Controls.Add(treeView);
		    SetcallTreeView();
		    this.Visible = true;
		}


	    public ArrayList FetchKids(TreeNodeBase nodebase)
		{
            Debug.Assert(nodebase.allkids != null);
			return nodebase.allkids;
		}
		private void SortOn(object obj, EventArgs e)
		{
			ColumnInformation ci = ((DiffColumn)obj).ColumnInformation;
			if(viewState.sort.CounterId == ci.Token)
			{
				viewState.sort.SortingOrder *= -1;
			}
			else
			{
				viewState.sort.CounterId = ci.Token;
				viewState.sort.SortingOrder = (viewState.sort.CounterId == -1 ? -1 : 1);
			}
			diffCallTreeView.Resort();
		}
		private void SetcallTreeView()
		{
			foreach(Control c in controlCollection.Controls)
			{
				DiffTreeListView v = null;
				try
				{
					v = (DiffTreeListView)c;
				}
				catch
				{
					/* not interested in exceptions */
				}

				if(v != null)
				{
					diffCallTreeView = v;
					viewState = (ViewState)v.TokenObject;
					return;
				}
			}
			Debug.Fail("Cannot find tree view on the tab page");
		}
	
		private readonly Color[] colors =
        {
		    Color.Black,
		    Color.Green,
		    Color.BlueViolet,
		    Color.White,
		    Color.Yellow,
		    Color.Beige
		};
		
		public Color GetColor(TreeNodeBase root, bool positive)
		{
			var node = (DiffDataNode)root;
			int idx = (int)node.nodetype + (positive ? 0 : 3);
            return colors[idx];
		}

		
		/* returns font used to display the item (part of the ITreeOwner interface) */
		public Font GetFont(TreeNodeBase in_node)
		{
			var node = (DiffDataNode)in_node;
			FontStyle fs = FontStyle.Regular;
			if(node.data.firstTimeBroughtIn)
			{
				fs |= FontStyle.Italic;
			}
			if(node.highlighted)
			{
				fs |= FontStyle.Bold;
			}
			return (fs == FontStyle.Regular ? defaultFont : new Font(defaultFont, fs));

			
		}

		/* sort nodes at the branch level */
		public ArrayList ProcessNodes(ArrayList nodes)
		{
			bool add = false;
			var nodesAtOneLevel = new ArrayList();
			foreach(DiffDataNode node in nodes)
			{
				switch(node.nodetype)
				{
					case DiffDataNode.NodeType.Call:
						add = true;
						break;

					case DiffDataNode.NodeType.Allocation:
						add = viewState.showAllocs;
						break;

					case DiffDataNode.NodeType.AssemblyLoad:
						add = viewState.showAssemblies;
						break;
				}

				if(add)
				{
					nodesAtOneLevel.Add(node);
				}
			}
			if(nodesAtOneLevel.Count == 0)
			{
				return nodesAtOneLevel;
			}

			/* sort nodes first */
			nodesAtOneLevel.Sort(this);

			/* then choose the nodes to highlight */
			SortingBehaviour ss = viewState.sort;
			/* this is needed to use the default Compare method */
			viewState.sort = viewState.highlight;
			var nodesToHighlight = new ArrayList();
			var currentBest = (DiffDataNode)nodesAtOneLevel[0];

			currentBest.highlighted = false;
			nodesToHighlight.Add(currentBest);
			for(int i = 1; i < nodesAtOneLevel.Count; i++)
			{
				var n = (DiffDataNode)nodesAtOneLevel[i];
				n.highlighted = false;

				int res = Compare(currentBest, n) * viewState.highlight.SortingOrder;
				if(res == 0)
				{
					nodesToHighlight.Add(n);
				}
				else if(res > 0)
				{
					currentBest = n;
					nodesToHighlight.Clear();
					nodesToHighlight.Add(currentBest);
				}
			}
			viewState.sort = ss;

			foreach(DiffDataNode n in nodesToHighlight)
			{
				n.highlighted = true;
			}

			/* reverse order if required */
			if(viewState.sort.SortingOrder > 0)
			{
				nodesAtOneLevel.Reverse();
			}
			return nodesAtOneLevel;
		}

		/* implements IComparer that compares the nodes according to the current sorting order */
		public int Compare(object x, object y)
		{
			var a = (DiffDataNode)x;
			var b = (DiffDataNode)y;

			if(viewState.sort.CounterId == -1)
			{
				// compare based on the invokation order
				//return a.prevOffset.CompareTo(b.prevOffset);
				return a.nodeId.CompareTo(b.nodeId);
			}

			var aa = (IComparable)GetInfo(a, viewState.sort.CounterId);
			var bb = (IComparable)GetInfo(b, viewState.sort.CounterId);
			try
			{
				return aa.CompareTo(bb);
			}
			catch
			{
				/* if string ("" is used instead of 0) is being compared against a number */
				bool aazero = (aa.ToString() == "");
				bool bbzero = (bb.ToString() == "");
				return aazero && bbzero ? 0 : aazero ? -1 : 1;
			}
		}
		
		/* returns data about the item for a given counter.
		 * object's ToString() is used to display that data */
		private object GetInfo(TreeNodeBase node, int counterId)
		{
			long number;
			var root = (DiffDataNode)node;
			if(counterId < 0)
			{
				//return MakeName(root);
				return root.name;
			}
			else
			{
				number = root.data.GetCounterValue(counterId);
			}
			/* use empty string to denote `0` */
			if(number == 0)
			{
				return "";
			}
			return number;
		}

		/* returns data about the item for a given counter.
		 * object's ToString() is used to display that data */
		public object GetInfo(TreeNodeBase node, ColumnInformation info)
		{
			return GetInfo(node, info == null ? -1 : info.Token);
		}

		#region GUI function
		private DiffColumn AddColumn(DiffTreeListView treeView, int counterId)
		{
			DiffColumn c = treeView.AddColumn(new ColumnInformation(counterId,
				DiffStatistics.GetCounterName(counterId),
				ColumnInformation.ColumnTypes.String),
				60);
			if(DiffStatistics.IsInclusive(counterId))
			{
				c.Font = new Font(c.Font, FontStyle.Bold);
			}
			return c;
		}
		#endregion

		private void DiffCallTreeForm_Resize(object sender, System.EventArgs e)
		{
			controlCollection.SuspendLayout();
			controlCollection.Left = 0;
			controlCollection.Top = 0;
			controlCollection.Width = this.ClientSize.Width;
			controlCollection.Height = this.ClientSize.Height - controlCollection.Top;
			controlCollection.ResumeLayout();
		}

				
		private void DiffCallTreeForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			_allocDiff.RefreshCallTreeNodes(_allocDiff.Root);
		}
	}
}
