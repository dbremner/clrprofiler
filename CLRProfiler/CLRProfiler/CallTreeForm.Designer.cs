using System;
using System.IO;
using System.Text;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ComponentModel;
using System.Windows.Forms;
using Microsoft.Win32;
namespace CLRProfiler
{
	internal sealed partial class CallTreeForm : System.Windows.Forms.Form, IComparer, ITreeOwner
	{

        /* random stuff */
        private Label threadIDLabel;
        private ComboBox threadIDList;

        
        /* controls */
        private System.Windows.Forms.Button allFunctionsButton;
        private System.Windows.Forms.Panel controlCollection;
        private System.Windows.Forms.Button allAllocationsButton;
        private System.Windows.Forms.ListView stackView;
        private System.Windows.Forms.Splitter splitter;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem selectColumns;

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MenuItem menuItem4;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem menuItem6;
        private System.Windows.Forms.MenuItem menuItem7;
        private System.Windows.Forms.MenuItem menuItemShowFuture;
        private System.Windows.Forms.MenuItem menuItemCopyStack;

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
            this.threadIDLabel = new System.Windows.Forms.Label();
            this.threadIDList = new System.Windows.Forms.ComboBox();
            this.allFunctionsButton = new System.Windows.Forms.Button();
            this.allAllocationsButton = new System.Windows.Forms.Button();
            this.controlCollection = new System.Windows.Forms.Panel();
            this.stackView = new System.Windows.Forms.ListView();
            this.splitter = new System.Windows.Forms.Splitter();
            this.mainMenu1 = new System.Windows.Forms.MainMenu();
            this.menuItem4 = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem6 = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.selectColumns = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.menuItem7 = new System.Windows.Forms.MenuItem();
            this.menuItemShowFuture = new System.Windows.Forms.MenuItem();
            this.menuItemCopyStack = new System.Windows.Forms.MenuItem();
            this.controlCollection.SuspendLayout();
            this.SuspendLayout();
            // 
            // threadIDLabel
            // 
            this.threadIDLabel.Location = new System.Drawing.Point(0, 0);
            this.threadIDLabel.Name = "threadIDLabel";
            this.threadIDLabel.TabIndex = 0;
            // 
            // threadIDList
            // 
            this.threadIDList.Location = new System.Drawing.Point(0, 0);
            this.threadIDList.Name = "threadIDList";
            this.threadIDList.Size = new System.Drawing.Size(121, 21);
            this.threadIDList.TabIndex = 0;
            // 
            // allFunctionsButton
            // 
            this.allFunctionsButton.Location = new System.Drawing.Point(200, 8);
            this.allFunctionsButton.Name = "allFunctionsButton";
            this.allFunctionsButton.Size = new System.Drawing.Size(136, 21);
            this.allFunctionsButton.TabIndex = 3;
            this.allFunctionsButton.Text = "Display All Functions...";
            // 
            // allAllocationsButton
            // 
            this.allAllocationsButton.Location = new System.Drawing.Point(352, 8);
            this.allAllocationsButton.Name = "allAllocationsButton";
            this.allAllocationsButton.Size = new System.Drawing.Size(136, 21);
            this.allAllocationsButton.TabIndex = 4;
            this.allAllocationsButton.Text = "Display All Allocations...";
            // 
            // controlCollection
            // 
            this.controlCollection.Controls.Add(this.stackView);
            this.controlCollection.Controls.Add(this.splitter);
            this.controlCollection.Location = new System.Drawing.Point(16, 40);
            this.controlCollection.Name = "controlCollection";
            this.controlCollection.Size = new System.Drawing.Size(400, 288);
            this.controlCollection.TabIndex = 5;
            // 
            // stackView
            // 
            this.stackView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.stackView.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204)));
            this.stackView.FullRowSelect = true;
            this.stackView.GridLines = true;
            this.stackView.Location = new System.Drawing.Point(4, 0);
            this.stackView.Name = "stackView";
            this.stackView.Size = new System.Drawing.Size(396, 288);
            this.stackView.TabIndex = 1;
            this.stackView.View = System.Windows.Forms.View.Details;
            this.stackView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.stackView_MouseDown);
            this.stackView.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.stackView_ColumnClick);
            // 
            // splitter
            // 
            this.splitter.Location = new System.Drawing.Point(0, 0);
            this.splitter.Name = "splitter";
            this.splitter.Size = new System.Drawing.Size(4, 288);
            this.splitter.TabIndex = 1;
            this.splitter.TabStop = false;
            this.splitter.SplitterMoved += new System.Windows.Forms.SplitterEventHandler(this.splitter_SplitterMoved);
            // 
            // mainMenu1
            // 
            this.mainMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem4,
                                                                                      this.menuItem1});
            // 
            // menuItem4
            // 
            this.menuItem4.Index = 0;
            this.menuItem4.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.menuItem5,
                                                                                      this.menuItem6});
            this.menuItem4.Text = "&View";
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 0;
            this.menuItem5.Text = "All &functions";
            this.menuItem5.Click += new System.EventHandler(this.menuItem5_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.Index = 1;
            this.menuItem6.Text = "All &objects";
            this.menuItem6.Click += new System.EventHandler(this.menuItem6_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 1;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
                                                                                      this.selectColumns,
                                                                                      this.menuItem2,
                                                                                      this.menuItem3,
                                                                                      this.menuItem7,
                                                                                      this.menuItemShowFuture,
                                                                                      this.menuItemCopyStack});
            this.menuItem1.Text = "&Options";
            // 
            // selectColumns
            // 
            this.selectColumns.Index = 0;
            this.selectColumns.Text = "Select &columns...";
            this.selectColumns.Click += new System.EventHandler(this.selectColumns_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 1;
            this.menuItem2.Text = "&Sort options...";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 2;
            this.menuItem3.Text = "&Filtering...";
            this.menuItem3.Click += new System.EventHandler(this.menuItem3_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.Index = 3;
            this.menuItem7.Text = "Filter F&unctions...";
            this.menuItem7.Click += new System.EventHandler(this.menuItem7_Click);
            // 
            // menuItemShowFuture
            // 
            this.menuItemShowFuture.Index = 4;
            this.menuItemShowFuture.Text = "Show Su&btree in Stack Window";
            this.menuItemShowFuture.Click += new System.EventHandler(this.menuItem8_Click);

            // 
            // menuItemCopyStack
            // 
            this.menuItemCopyStack.Index = 5;
            this.menuItemCopyStack.Text = "Copy Stac&k View";
            this.menuItemCopyStack.Click += new System.EventHandler(this.menuItem9_Click);

            // 
            // CallTreeForm
            // 
            this.ClientSize = new System.Drawing.Size(632, 493);
            this.Controls.Add(this.controlCollection);
            this.Menu = this.mainMenu1;
            this.Name = "CallTreeForm";
            this.Text = "Call Tree View";
            this.Resize += new System.EventHandler(this.CallTreeForm_Resize);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.CallTreeForm_Closing);
            this.controlCollection.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
	}
}

