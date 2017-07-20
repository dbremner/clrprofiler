using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
namespace CLRProfiler
{
	internal sealed partial class FunctionFind : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.TextBox tbSearchString;
		private System.Windows.Forms.ListBox lbFunctions;
		private System.Windows.Forms.Button btnSearch;
		private System.Windows.Forms.Button btnOk;
		private System.Windows.Forms.Button btnCancel;
		private System.Windows.Forms.Label label2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.label1 = new System.Windows.Forms.Label();
			this.tbSearchString = new System.Windows.Forms.TextBox();
			this.lbFunctions = new System.Windows.Forms.ListBox();
			this.btnSearch = new System.Windows.Forms.Button();
			this.btnOk = new System.Windows.Forms.Button();
			this.btnCancel = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(24, 24);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(248, 32);
			this.label1.TabIndex = 0;
			this.label1.Text = "Enter a search string, then click the \'Search\' button.";
			// 
			// tbSearchString
			// 
			this.tbSearchString.Location = new System.Drawing.Point(24, 56);
			this.tbSearchString.Name = "tbSearchString";
			this.tbSearchString.Size = new System.Drawing.Size(240, 20);
			this.tbSearchString.TabIndex = 1;
			this.tbSearchString.Text = "";
			// 
			// lbFunctions
			// 
			this.lbFunctions.Location = new System.Drawing.Point(24, 136);
			this.lbFunctions.Name = "lbFunctions";
			this.lbFunctions.Size = new System.Drawing.Size(240, 82);
			this.lbFunctions.TabIndex = 2;
			this.lbFunctions.SelectedIndexChanged += new System.EventHandler(this.lbFunctions_SelectedIndexChanged);
			// 
			// btnSearch
			// 
			this.btnSearch.Location = new System.Drawing.Point(72, 96);
			this.btnSearch.Name = "btnSearch";
			this.btnSearch.Size = new System.Drawing.Size(136, 24);
			this.btnSearch.TabIndex = 3;
			this.btnSearch.Text = "Search";
			this.btnSearch.Click += new System.EventHandler(this.btnSearch_Click);
			// 
			// btnOk
			// 
			this.btnOk.Location = new System.Drawing.Point(40, 288);
			this.btnOk.Name = "btnOk";
			this.btnOk.Size = new System.Drawing.Size(56, 24);
			this.btnOk.TabIndex = 4;
			this.btnOk.Text = "OK";
			this.btnOk.Click += new System.EventHandler(this.btnOk_Click);
			// 
			// btnCancel
			// 
			this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.btnCancel.Location = new System.Drawing.Point(160, 288);
			this.btnCancel.Name = "btnCancel";
			this.btnCancel.Size = new System.Drawing.Size(56, 24);
			this.btnCancel.TabIndex = 5;
			this.btnCancel.Text = "Cancel";
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(32, 240);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(232, 24);
			this.label2.TabIndex = 6;
			this.label2.Text = "Select your function from the list above.";
			// 
			// FunctionFind
			// 
			this.AcceptButton = this.btnSearch;
			this.CancelButton = this.btnCancel;
			this.ClientSize = new System.Drawing.Size(292, 326);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.btnCancel);
			this.Controls.Add(this.btnOk);
			this.Controls.Add(this.btnSearch);
			this.Controls.Add(this.lbFunctions);
			this.Controls.Add(this.tbSearchString);
			this.Controls.Add(this.label1);
			this.Name = "FunctionFind";
			this.Text = "FunctionFind";
			this.ResumeLayout(false);

		}
		#endregion
	}
}

