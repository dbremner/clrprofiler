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
	internal sealed partial class DiffCallTreeForm : System.Windows.Forms.Form, IComparer, IDiffTreeOwner
	{
		

		private System.Windows.Forms.Panel controlCollection;
		
		/// <summary>
		/// Required designer variable.
		/// </summary>
		/// 
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
			this.controlCollection = new System.Windows.Forms.Panel();
			this.SuspendLayout();
			// 
			// controlCollection
			// 
			this.controlCollection.Dock = System.Windows.Forms.DockStyle.Fill;
			this.controlCollection.Location = new System.Drawing.Point(0, 0);
			this.controlCollection.Name = "controlCollection";
			this.controlCollection.Size = new System.Drawing.Size(632, 446);
			this.controlCollection.TabIndex = 0;
			// 
			// DiffCallTreeForm
			// 
			this.ClientSize = new System.Drawing.Size(632, 446);
			this.Controls.Add(this.controlCollection);
			this.Name = "DiffCallTreeForm";
			this.Text = "DiffCallTreeForm";
			this.Resize += new System.EventHandler(this.DiffCallTreeForm_Resize);
			this.Closing += new System.ComponentModel.CancelEventHandler(this.DiffCallTreeForm_Closing);
			this.ResumeLayout(false);

		}
		#endregion
	}
}

