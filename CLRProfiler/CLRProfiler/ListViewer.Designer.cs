using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
namespace CLRProfiler
{
	public partial class ListViewer : System.Windows.Forms.Form, IComparer
	{
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem2;
        public System.Windows.Forms.ListView list;
        private IContainer components;

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
            this.components = new System.ComponentModel.Container();
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.list = new System.Windows.Forms.ListView();
            this.SuspendLayout();
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem2});
            this.menuItem1.Text = "&File";
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 0;
            this.menuItem2.Text = "&Save as text...";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // list
            // 
            this.list.AllowColumnReorder = true;
            this.list.Dock = System.Windows.Forms.DockStyle.Fill;
            this.list.FullRowSelect = true;
            this.list.GridLines = true;
            this.list.Location = new System.Drawing.Point(0, 0);
            this.list.Name = "list";
            this.list.Size = new System.Drawing.Size(292, 273);
            this.list.TabIndex = 0;
            this.list.UseCompatibleStateImageBehavior = false;
            this.list.View = System.Windows.Forms.View.Details;
            this.list.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.list_ColumnClick);
            // 
            // ListViewer
            // 
            this.ClientSize = new System.Drawing.Size(292, 273);
            this.Controls.Add(this.list);
            this.Menu = this.mainMenu1;
            this.Name = "ListViewer";
            this.Text = "ListViewer";
            this.ResumeLayout(false);

        }
        #endregion
	}
}

