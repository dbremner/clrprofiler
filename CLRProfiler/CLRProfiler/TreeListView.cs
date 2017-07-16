/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  TreeListView, TreeNodeBase, ITreeOwner
 *
 * Description: A hybrid between the TreeView and ListView classes
 */

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal class TreeListView : Control
	{
		private int leftEdge;

	    [NotNull] private readonly ArrayList columns;
	    [NotNull] private readonly ListBox treeListBox;
	    [NotNull] private readonly ResizeBarCapture resizeBar;

		private object keepSelected;
	    [NotNull] private readonly PlacedToolTip hoverPopup;

		// events of interest
		internal event EventHandler ColumnClick;
		internal event EventHandler SelectedIndexChanged;

		internal object TokenObject = null;
	    [NotNull] private readonly ITreeOwner treeOwner;

		// item in treeListBox for current context menu
		private int ContextSelection;

		private TreeNodeBase root;
		internal TreeNodeBase Root
		{
			set
			{
				if(value != null)
				{
					treeListBox.Items.Clear();
					treeListBox.Items.Add(root = value);
					root.depth = 0;
				}
			}
			get
			{
				return root;
			}
		}

		enum Direction
		{
			Forward,
			Backward
		}

		internal ListBox.ObjectCollection Items => treeListBox.Items;

	    internal object SelectedItem
		{
			get => treeListBox.SelectedItem;
		    set => treeListBox.SelectedItem = value;
		}

		private void Resort(int depth, [NotNull] TreeNodeBase root)
		{
			root.depth = depth;
			treeListBox.Items.Add(root);
			if(root.allkids != null && root.IsExpanded)
			{
				var nodes = treeOwner.ProcessNodes(TokenObject, root.allkids);
				for(int i = 0; i < nodes.Count; i++)
				{
					Resort(1 + depth, (TreeNodeBase)nodes[i]);
				}
			}
		}

		internal void Resort()
		{
			keepSelected = treeListBox.SelectedItem;
			treeListBox.Items.Clear();
			Resort(0, root);
		}

		private void ReplaceContents([NotNull] object[] nodes)
		{
			/* an attempt to prevent the control from jerking */
			object selection = treeListBox.Items[treeListBox.SelectedIndex];
			int topIndex = treeListBox.TopIndex;
			int leftEdge = GetScrollPos(treeListBox.Handle, 0);

			treeListBox.BeginUpdate();
			treeListBox.Items.Clear();
			treeListBox.Items.AddRange(nodes);
			treeListBox.SelectedItem = selection;
			treeListBox.TopIndex = topIndex;
			treeListBox.EndUpdate();

			/* send WM_HSCROLL message to the list box */
			SendMessage(treeListBox.Handle, 0x0114, 4 + (leftEdge << 16), 0);
		}

		private int AddAfter(int index, [NotNull] TreeNodeBase root)
		{
			if(root.allkids == null)
			{
				root.allkids = treeOwner.FetchKids(TokenObject, root);
			}
			var nodes = treeOwner.ProcessNodes(TokenObject, root.allkids);

			int numNodes = nodes.Count, nodesInList = treeListBox.Items.Count;
			int costOfRebuilding = 3 * (numNodes + nodesInList);
			int costOfInsertions = (1 + nodesInList - index) * numNodes;
		    int newDepth = 1 + root.depth;

		    if(costOfInsertions < costOfRebuilding)
			{
				foreach(TreeNodeBase curr in nodes)
				{
					curr.depth = newDepth;
					treeListBox.Items.Insert(++index, curr);
				}
			}
			else
			{
				++index;
				ListBox.ObjectCollection items = treeListBox.Items;
				object[] allNodes = new object[numNodes + nodesInList];
			    int i;
			    for(i = 0; i < index; i++)
				{
					allNodes[i] = items[i];
				}
				for(; i < index + numNodes; i++)
				{
					allNodes[i] = nodes[i - index];
					((TreeNodeBase)allNodes[i]).depth = newDepth;
				}
				for(; i < numNodes + nodesInList; i++)
				{
					allNodes[i] = items[i - numNodes];
				}
				ReplaceContents(allNodes);
			}
			return index;
		}

		override protected void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			treeListBox.Font = Font;
			treeListBox.ItemHeight = Font.Height + 3;
		}

		internal TreeListView([NotNull] ITreeOwner in_treeOwner)
		{
			treeOwner = in_treeOwner;

			hoverPopup = new PlacedToolTip();
			hoverPopup.Parent = FindForm();

			treeListBox = new ListBox();
			treeListBox.Parent = this;
			treeListBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
			treeListBox.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
			treeListBox.IntegralHeight = false;
			treeListBox.Name = "treeListBox";
			treeListBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.treeListBox_KeyDown);
			treeListBox.MouseDown += new System.Windows.Forms.MouseEventHandler(this.treeListBox_MouseDown);
			treeListBox.MouseMove += new System.Windows.Forms.MouseEventHandler(this.treeListBox_MouseMove);
			treeListBox.DoubleClick += new System.EventHandler(this.treeListBox_DoubleClick);
			treeListBox.DrawItem += new System.Windows.Forms.DrawItemEventHandler(this.treeListBox_DrawItem);
			treeListBox.SelectedIndexChanged += new System.EventHandler(this.treeListBox_SelectedIndexChanged);

			columns = new ArrayList();

			// must be created before calling `AddColumn`
			resizeBar = new ResizeBarCapture(columns);
			resizeBar.Visible = false;
			resizeBar.Parent = this;

			leftEdge = 0;

			OnResize(null);

			// log = new StreamWriter("test.log");

			// Create a blank context menu.  We'll fill it in when the user right clicks
			var contextMenu = new ContextMenu();
			ContextMenu = contextMenu;
		}

		// StreamWriter log = null;

		internal void RepaintTreeView()
		{
			RedoColumnLayout();
			treeListBox.Invalidate();
			treeListBox.Update();
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose( disposing );
		}

		void RedoColumnLayout()
		{
			int position = -leftEdge;
			int height = Font.Height + 5;
			for(IEnumerator e = columns.GetEnumerator(); e.MoveNext();)
			{
				var current = (Column)e.Current;
				current.SuspendLayout();
				current.Location = new Point(position, 0);
				if(height > 0)
				{
					current.Height = height;
				}
				current.ResumeLayout();
				position += current.Width;
			}

			position += leftEdge;
			treeListBox.HorizontalScrollbar = (position > treeListBox.ClientSize.Width);
			treeListBox.HorizontalExtent = position;
		}

		[DllImport("User32.Dll")]
		internal static extern int GetScrollPos(IntPtr hwnd, int bar);

		[DllImport("User32.Dll")]
		internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

		private void treeListBox_DrawItem(object sender, System.Windows.Forms.DrawItemEventArgs e)
		{
			/* 0 stands for SB_HORZ */
			int leftEdge = GetScrollPos(treeListBox.Handle, 0);
			if(leftEdge != this.leftEdge)
			{
				this.leftEdge = leftEdge;
				RedoColumnLayout();
			}

			int position = 0;

			Graphics g = e.Graphics;
			ListBox treeView = treeListBox;

			if(e.Index < 0 || e.Index >= treeView.Items.Count)
			{
				return;
			}

		    var sf = new StringFormat(StringFormatFlags.NoWrap)
		    {
		        Trimming = StringTrimming.EllipsisCharacter
		    };

		    var node = (TreeNodeBase)Items[e.Index];

			int crossover = (treeListBox.ItemHeight - 1) * (1 + node.depth);
			g.FillRectangle(new SolidBrush(Color.White), position, e.Bounds.Top, crossover, e.Bounds.Height);

			var itemRect = new Rectangle(crossover, e.Bounds.Top, e.Bounds.Right - crossover, e.Bounds.Height);
			g.FillRectangle(new SolidBrush(e.BackColor), itemRect);

			if(e.State == DrawItemState.Focus)
			{
				ControlPaint.DrawFocusRectangle(g, itemRect, e.ForeColor, e.BackColor);
			}

			var grayPen = Pens.LightGray;
			g.DrawLine(grayPen, 0, e.Bounds.Bottom - 1, e.Bounds.Right, e.Bounds.Bottom - 1);

			Font fontToUse = treeOwner.GetFont(TokenObject, node);
			Color color = treeOwner.GetColor(TokenObject, node, (e.State & DrawItemState.Selected) != DrawItemState.Selected);

			Brush brush = new SolidBrush(color);

			Region oldClip = g.Clip;
			foreach(Column c in columns)
			{
				ColumnInformation current = c.ColumnInformation;
				var rect = new Rectangle(position, e.Bounds.Top, c.Width, e.Bounds.Height);
				g.Clip = new Region(rect);

				string res = treeOwner.GetInfo(TokenObject, node, current).ToString();

				if(current.ColumnType == ColumnInformation.ColumnTypes.Tree)
				{
					rect.Offset((1 + node.depth) * (treeListBox.ItemHeight - 1), 0);
					rect.Width -= (1 + node.depth) * (treeListBox.ItemHeight - 1);

					if(node.HasKids)
					{
						var p = Pens.Gray;
						int y0 = e.Bounds.Top;
						int x0 = position + node.depth * (treeListBox.ItemHeight - 1);
						g.DrawRectangle(p, x0 + 3, y0 + 3, (treeListBox.ItemHeight - 9), (treeListBox.ItemHeight - 9));
						g.DrawLine(p, x0 + 5, y0 + (treeListBox.ItemHeight - 3) / 2, x0 + (treeListBox.ItemHeight - 8), y0 + (treeListBox.ItemHeight - 3) / 2);
						if(!node.IsExpanded)
						{
							g.DrawLine(p, x0 + (treeListBox.ItemHeight - 3) / 2, y0 + 5, x0 + (treeListBox.ItemHeight - 3) / 2, y0 + (treeListBox.ItemHeight - 8));
						}
					}
				}

				if(res != null)
				{
					int characters;

					var layoutArea = new SizeF(rect.Width, rect.Height);
					SizeF stringSize = g.MeasureString(res, fontToUse, layoutArea, sf, out characters, out _);

					g.DrawString(res.Substring(0, characters) + (characters < res.Length ? "..." : ""), fontToUse, brush, rect.Location, sf);
					g.DrawLine(grayPen, rect.Right - 1, e.Bounds.Top, rect.Right - 1, e.Bounds.Bottom - 1);
				}

				position += c.Width;
			}
			g.Clip = oldClip;
		}

		private void treeListBox_MouseMove(object s, MouseEventArgs e)
		{
			int index = treeListBox.IndexFromPoint(e.X, e.Y);
			if(index < 0 || index >= Items.Count)
			{
				return;
			}

			var node = (TreeNodeBase)Items[index];
			int crossover = (treeListBox.ItemHeight - 1) * (1 + node.depth) - leftEdge;
			Rectangle rect = treeListBox.GetItemRectangle(index);

			Point screenPoint = treeListBox.PointToScreen(new Point(crossover, rect.Top));
			if(hoverPopup.Parent == null)
			{
				hoverPopup.Parent = FindForm();
			}
		    Debug.Assert(hoverPopup.Parent != null);
            Point controlPoint = hoverPopup.Parent.PointToClient(screenPoint);

			string textToDisplay = treeOwner.GetInfo(TokenObject, node, null).ToString();
			if(textToDisplay != hoverPopup.CurrentlyDisplayed())
			{
				hoverPopup.Display(controlPoint, treeOwner.GetFont(TokenObject, node), rect.Height, textToDisplay);
			}
		}

		private void treeListBox_MouseDown(object s, MouseEventArgs e)
		{
			int index = treeListBox.IndexFromPoint(e.X, e.Y);
			if(index == ListBox.NoMatches)
			{
				return;
			}

			var node = (TreeNodeBase)Items[index];
			if (e.Button == MouseButtons.Left)
			{
				int offset = node.depth * (treeListBox.ItemHeight - 1) - leftEdge;
				if(offset <= e.X && e.X < offset + (treeListBox.ItemHeight - 1))
				{
					ToggleBranch(index);
				}
			}
			else
			{
				//  Customize the context menu
				ContextMenu contextMenu = this.ContextMenu;
				ColumnInformation ci = ((Column)columns[0]).ColumnInformation;
				string fnName = treeOwner.GetInfo(TokenObject, node, ci).ToString();
				String strFn;

			    ContextSelection = index;
				EventHandler eventHandler = new EventHandler( this.ContextMenu_Selection );
				contextMenu.MenuItems.Clear();
				contextMenu.MenuItems.Add( new MenuItem( "Find...", eventHandler));
				contextMenu.MenuItems.Add( new MenuItem( "Find " + fnName + " forward", eventHandler));
				contextMenu.MenuItems.Add( new MenuItem( "Find " + fnName + " backward", eventHandler));
				
				CallTreeForm.FnViewFilter[] filterFns = treeOwner.GetIncludeFilters();				
				for (int i = 0; i < 2; i++)
				{
					if (filterFns[i].functionId > 0)
					{
						if (filterFns[i].nodetype == TreeNode.NodeType.Call)
                        {
                            strFn = treeOwner.MakeNameForFunction( filterFns[i].functionId );
                        }
                        else
                        {
                            strFn = treeOwner.MakeNameForAllocation( filterFns[i].functionId, 0);
                        }
                    }
					else
					{
						strFn = "none";
					}
					contextMenu.MenuItems.Add( new MenuItem( "Set Include filter " + (i+1).ToString() + " (" + strFn + ")", eventHandler));
				}

				filterFns = treeOwner.GetExcludeFilters();				
				for (int i = 0; i < 2; i++)
				{
					if (filterFns[i].functionId > 0)
					{
						if (filterFns[i].nodetype == TreeNode.NodeType.Call)
                        {
                            strFn = treeOwner.MakeNameForFunction( filterFns[i].functionId );
                        }
                        else
                        {
                            strFn = treeOwner.MakeNameForAllocation( filterFns[i].functionId, 0 );
                        }
                    }
					else
					{
						strFn = "none";
					}
					contextMenu.MenuItems.Add( new MenuItem( "Set Exclude filter " + (i+1).ToString() + " (" + strFn + ")", eventHandler));
				}

				contextMenu.MenuItems.Add( new MenuItem( "Clear Filters" , eventHandler ) );
				contextMenu.MenuItems.Add( new MenuItem( "Regenerate Tree" , eventHandler ) );
			}
		}

		private void ContextMenu_Selection(object sender, System.EventArgs e) 
		{
			var miClicked = (MenuItem)sender;

			switch (miClicked.Index)
			{

				case 0:
					// Find ...
					var dlgFnFind = new FunctionFind( treeOwner, "" );
					if (dlgFnFind.ShowDialog() == DialogResult.OK)
					{
					    // Find dlgFn
					    int findId = dlgFnFind.SelectedFunctionId;
					    FindNode( ContextSelection, TreeNode.NodeType.Call, findId, Direction.Forward );
					}
					break;

				case 1:
				case 2:
					// Find selection forward (1) or backward (2)
					TreeNodeBase node;
					TreeNode.NodeType nodeType;
					int idCurrent;

					node = (TreeNodeBase)treeListBox.Items[ ContextSelection ];
					nodeType = ((TreeNode)node).nodetype;
					idCurrent = treeOwner.GetNodeId( node );
					FindNode( ContextSelection, nodeType, idCurrent, miClicked.Index == 1 ? Direction.Forward : Direction.Backward );
					break;


				case 3:
				case 4:
				case 5:
				case 6:
					// Add function to filters
					CallTreeForm.FnViewFilter [] filters;

					// include or exclude filters?
					bool fIncludes = miClicked.Index == 3 || miClicked.Index == 4;
					int filterId = (miClicked.Index - 3) & 1;  // 0 or 1

					node = (TreeNodeBase)treeListBox.Items[ ContextSelection ];
					nodeType = ((TreeNode)node).nodetype;
					if (nodeType == TreeNode.NodeType.Call || nodeType == TreeNode.NodeType.Allocation)
					{
						idCurrent = treeOwner.GetNodeId( node );

						filters = fIncludes ? treeOwner.GetIncludeFilters() : treeOwner.GetExcludeFilters();
						filters[ filterId ].nodetype = nodeType;
						filters[ filterId ].functionId = idCurrent;
					}
					else
					{
						MessageBox.Show("Can only filter calls or allocations.  Please select a function.");
					}
					break;

				case 7:
					//  Reset filters
					filters = treeOwner.GetIncludeFilters();
					filters[0].functionId = -1;
					filters[1].functionId = -1;

					filters = treeOwner.GetExcludeFilters();
					filters[0].functionId = -1;
					filters[1].functionId = -1;
					break;

				case 8:
					//  Regenerate tree
					treeOwner.RegenerateTree();
					break;
			}


		}

		private void FindNode( int StartId, TreeNode.NodeType nodeType, int targetId, Direction direction )
		{
			int curSel = StartId;

		    while(true)
			{
			    TreeNode node;
			    if (direction == Direction.Forward)
				{
					curSel++;
					if (curSel >= treeListBox.Items.Count)
                    {
                        curSel = -1;
                    }
                    else
					{
						//  Expand if not already expanded
						//  Note that we expand only on forward search
						node = (TreeNode)Items[curSel];
						if (node.HasKids && !node.IsExpanded)
						{
							ToggleBranch( curSel );
						}
					}
				}
				else
				{
					curSel--;
				}
				
				if (curSel == -1)
				{
					MessageBox.Show( "Cannot find function");
					break;
				}

				node = (TreeNode)Items[curSel];
				int functionId = treeOwner.GetNodeId( node );
				if (functionId == targetId && node.nodetype == nodeType)
				{
					treeListBox.SelectedIndex = curSel;
					break;
				}
			}
		}

		private void treeListBox_DoubleClick(object s, EventArgs e)
		{
			int index = treeListBox.SelectedIndex;
			if(index != -1)
			{
				ToggleBranch(index);
			}
		}

		override protected void OnResize(EventArgs e)
		{
			treeListBox.Size = new Size(ClientSize.Width,
										ClientSize.Height - treeListBox.Top);
			treeListBox.Refresh();
		}

		private void treeListBox_KeyDown(object s, KeyEventArgs e)
		{
			int index = treeListBox.SelectedIndex;
			if(index == -1)
			{
				return;
			}

			Keys key = e.KeyCode;
			var node = (TreeNodeBase)Items[index];
			switch(key)
			{
				case Keys.Left:
					if(node.IsExpanded)
					{
						ToggleBranch(index);
					}
					break;

				case Keys.Right:
					if(!node.IsExpanded)
					{
						ToggleBranch(index);
					}
					break;

				case Keys.Space:
					ToggleBranch(index);
					break;
			}
		}

		private void treeListBox_SelectedIndexChanged(object sender, System.EventArgs e)
		{
            SelectedIndexChanged?.Invoke(sender, e);
        }

		private void Column_Click(object sender, EventArgs e)
		{
            ColumnClick?.Invoke(sender, e);
        }

	    [NotNull]
	    internal ArrayList GetColumns()
		{
			return columns;
		}

		internal Column AddColumn(ColumnInformation ci, int defaultWidth)
		{
			var c = new Column(ci, resizeBar, columns);

			c.Width = defaultWidth;
			c.Parent = this;
			c.TextAlign = ContentAlignment.MiddleLeft;
			c.Click += new EventHandler(Column_Click);
			c.Font = Font;
			columns.Add(c);

			RedoColumnLayout();

			treeListBox.SuspendLayout();
			treeListBox.Location = new Point(0, c.Height);
			treeListBox.Size = new Size(ClientSize.Width, ClientSize.Height - c.Height);
			treeListBox.ResumeLayout();

			return c;
		}

		void ToggleBranch(int index)
		{
			var node = (TreeNodeBase)Items[index];
			if(!node.HasKids)
			{
				return;
			}

			int count = Items.Count;
			if(node.IsExpanded)
			{
				index++;
				int upTo = index;
				TreeNodeBase kid = null;
				// eh, hopefully item access is cheap

				ListBox.ObjectCollection items = treeListBox.Items;

				while(upTo < count && (kid = (TreeNodeBase)items[upTo]).depth > node.depth)
				{
					kid.IsExpanded = false;
					upTo++;
				}

				int i, nodesInList = items.Count;
				int toRemove = upTo - index;
				int costOfRemoveAt = toRemove * (1 + nodesInList - upTo);
				int costOfRebuilding = 2 * nodesInList + 3 * (nodesInList - (upTo - index));

				if(costOfRemoveAt < costOfRebuilding)
				{
					for(i = upTo; i-- > index;)
					{
						items.RemoveAt(i);
					}
				}
				else
				{
					object[] allNodes = new object[nodesInList - toRemove];
					for(i = 0; i < index; i++)
					{
						allNodes[i] = items[i];
					}
					for(i = upTo; i < nodesInList; i++)
					{
						allNodes[i - toRemove] = items[i];
					}

					ReplaceContents(allNodes);
				}

				node.IsExpanded = false;
			}
			else
			{
				AddAfter(index, node);
				node.IsExpanded = true;
			}
		}
	}
}

