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
using JetBrains.Annotations;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for FunctionFind.
    /// </summary>
    internal sealed partial class FunctionFind : System.Windows.Forms.Form
	{
	    [NotNull] private readonly ITreeOwner TreeOwner;
		internal int SelectedFunctionId;
		internal TreeNode.NodeType SelectedNodeType;

		internal FunctionFind([NotNull] ITreeOwner treeOwner, [NotNull] string SearchString )
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			TreeOwner = treeOwner;
			tbSearchString.Text = SearchString;
		}


		private void btnSearch_Click(object sender, System.EventArgs e)
		{
			int i;
			LineItem lineItem;
			string fn;
			string matchString = tbSearchString.Text;

			lbFunctions.Items.Clear();

			//  Add functions
			for( i = 0; ; i++)
			{
				fn = TreeOwner.MakeNameForFunction(i);
				if (fn == null)
				{
					break;
				}

				if ( fn.IndexOf( matchString ) != -1 )
				{
					lineItem = new LineItem(i, fn, TreeNode.NodeType.Call);

					lbFunctions.Items.Add( lineItem );
				}
			}

			//  Add allocations
			for( i = 0; ; i++)
			{
				fn = TreeOwner.MakeNameForAllocation(i, 0);
				if (fn == null)
				{
					break;
				}

				if ( fn.IndexOf( matchString ) != -1 )
				{
					lineItem = new LineItem(i, fn, TreeNode.NodeType.Allocation);

					lbFunctions.Items.Add( lineItem );
				}
			}
		}

		private void btnOk_Click(object sender, System.EventArgs e)
		{
			if (lbFunctions.SelectedIndex < 0)
			{
				// Nothing selected
				if (lbFunctions.Items.Count == 0)
				{
					MessageBox.Show( "To populate the list box, enter a search string and click the search button." );
				}
				else
				{
					MessageBox.Show( "Please select a function from the list box." );
				}
				return;
			}

			SelectedFunctionId = ((LineItem)lbFunctions.SelectedItem).Id;
			SelectedNodeType = ((LineItem)lbFunctions.SelectedItem).NodeType;
			DialogResult = DialogResult.OK;
			Close();
		}

		private void lbFunctions_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			// User selected function.  Make the OK button the default button.
			this.AcceptButton = this.btnOk;
		}
	}
}
