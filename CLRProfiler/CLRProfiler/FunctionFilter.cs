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

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for FunctionFilter.
	/// </summary>
	internal sealed partial class DlgFunctionFilter : System.Windows.Forms.Form
	{

		private readonly ITreeOwner m_treeOwner;

		private readonly CallTreeForm.FnViewFilter[] includeFns;
		private readonly CallTreeForm.FnViewFilter[] excludeFns;

		private TreeNode node;

		internal DlgFunctionFilter( ITreeOwner TreeOwner )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			m_treeOwner = TreeOwner;
			node = new TreeNode(TreeNode.NodeType.Call, 0 );

			includeFns = m_treeOwner.GetIncludeFilters();
			tbIncludeFn0.Text = GetName( includeFns[0].nodetype, includeFns[0].functionId );
			tbIncludeFn0.Enabled = true;
			tbIncludeFn1.Text = GetName( includeFns[1].nodetype, includeFns[1].functionId );
			tbIncludeFn1.Enabled = true;

			excludeFns = m_treeOwner.GetExcludeFilters();
			tbExcludeFn0.Text = GetName( excludeFns[0].nodetype, excludeFns[0].functionId );
			tbExcludeFn0.Enabled = true;
			tbExcludeFn1.Text = GetName( excludeFns[1].nodetype, excludeFns[1].functionId );
			tbExcludeFn1.Enabled = true;
		}

		private string GetName( TreeNode.NodeType nodetype, int id )
		{
			if (id == -1)
			{
				return "";
			}
			else
			{
				if (nodetype == TreeNode.NodeType.Call)
                {
                    return m_treeOwner.MakeNameForFunction( id );
                }
                else
                {
                    return m_treeOwner.MakeNameForAllocation( id, 0 );
                }
            }
		}

		private void btnIncludeFn0_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbIncludeFn0 );
			if (viewFilter.functionId > 0)
			{
				includeFns[0] = viewFilter;
			}

		}

		private void btnIncludeFn1_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbIncludeFn1 );
			if (viewFilter.functionId > 0)
			{
				includeFns[1] = viewFilter;
			}
		
		}

		private void btnExcludeFn0_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbExcludeFn0 );
			if (viewFilter.functionId > 0)
			{
				excludeFns[0] = viewFilter;
			}
		
		}

		private void btnExcludeFn1_Click(object sender, System.EventArgs e)
		{
			CallTreeForm.FnViewFilter viewFilter = FindFunction( tbExcludeFn1 );
			if (viewFilter.functionId > 0)
			{
				excludeFns[1] = viewFilter;
			}
		}

		//
		//  Popup a dialog to let the user select a function name from 
		//  the list of all functions in the current view.
		//
		//  Returns:
		//     -1:  Dialog cancelled
		//	   >=0: Fn id.
		//

		private CallTreeForm.FnViewFilter FindFunction( TextBox tb )
		{
			int id = -2;
			TreeNode.NodeType nodetype = TreeNode.NodeType.Call;
			var functionFind = new FunctionFind( m_treeOwner, tb.Text );
			var viewFilter = new CallTreeForm.FnViewFilter();

			if (functionFind.ShowDialog() == DialogResult.OK)
			{
				id = functionFind.SelectedFunctionId;
				if (id >= 0)
				{
					nodetype = functionFind.SelectedNodeType;
					tb.Text = GetName( nodetype, id );
				}
			}

			viewFilter.functionId = id;
			viewFilter.nodetype = nodetype;

			return viewFilter;
		}

		private void btnClear_Click(object sender, System.EventArgs e)
		{
			includeFns[0].functionId = -1;
			includeFns[1].functionId = -1;
			excludeFns[0].functionId = -1;
			excludeFns[1].functionId = -1;

			tbIncludeFn0.Text = GetName( includeFns[0].nodetype, includeFns[0].functionId );
			tbIncludeFn1.Text = GetName( includeFns[1].nodetype, includeFns[1].functionId );
			tbExcludeFn0.Text = GetName( excludeFns[0].nodetype, excludeFns[0].functionId );
			tbExcludeFn1.Text = GetName( excludeFns[1].nodetype, excludeFns[1].functionId );
		}
		


	}
}
