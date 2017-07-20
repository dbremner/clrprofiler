using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Text;
namespace CLRProfiler
{
	internal sealed partial class GraphViewForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel graphPanel;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.GroupBox scaleGroupBox;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.RadioButton radioButton10;
        private System.Windows.Forms.RadioButton radioButton11;
        private System.Windows.Forms.RadioButton radioButton12;
        private System.Windows.Forms.RadioButton radioButton13;
        private System.Windows.Forms.RadioButton radioButton14;
        private System.Windows.Forms.RadioButton radioButton15;
        private System.Windows.Forms.RadioButton radioButton16;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem pruneContextMenuItem;
        private System.Windows.Forms.MenuItem selectRecursiveMenuItem;
        private System.Windows.Forms.MenuItem copyContextMenuItem;
        private System.Windows.Forms.Timer versionTimer;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.Panel outerPanel;
        private System.Windows.Forms.MenuItem filterMenuItem;
        private System.Windows.Forms.MenuItem selectAllMenuItem;
        private System.Windows.Forms.MenuItem findMenuItem;
        private System.Windows.Forms.MenuItem showWhoAllocatedMenuItem;
        private System.Windows.Forms.MenuItem findInterestingNodesMenuItem;
        private System.Windows.Forms.MainMenu mainMenu1;
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem selectAllMainMenuItem;
        private System.Windows.Forms.MenuItem copyMainMenuItem;
        private System.Windows.Forms.MenuItem findMainMenuItem;
        private System.Windows.Forms.MenuItem showWhoAllocatedNewMenuItem;
        private System.Windows.Forms.MenuItem showNewObjectsMenuItem;
        private System.Windows.Forms.MenuItem zoomToNodeMenuItem;
        private System.Windows.Forms.MenuItem showObjectsAllocatedBetween;
        private System.Windows.Forms.MenuItem showWhoAllocatedObjectsBetweenMenuItem;
        private System.Windows.Forms.MenuItem showInstancesMenuItem;
        private System.Windows.Forms.MenuItem showHistogramMenuItem;
        private System.Windows.Forms.MenuItem showReferencesMenuItem;
        private System.Windows.Forms.MenuItem filterToCallersCalleesMenuItem;
        private ToolTip toolTip;
        private System.Windows.Forms.MenuItem resetFilterMenuItem;
        private System.Windows.Forms.MenuItem findAgainMenuItem;
        private System.Windows.Forms.MenuItem findAgainMainMenuItem;

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
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.radioButton16 = new System.Windows.Forms.RadioButton();
            this.radioButton15 = new System.Windows.Forms.RadioButton();
            this.radioButton14 = new System.Windows.Forms.RadioButton();
            this.radioButton13 = new System.Windows.Forms.RadioButton();
            this.radioButton12 = new System.Windows.Forms.RadioButton();
            this.radioButton11 = new System.Windows.Forms.RadioButton();
            this.radioButton10 = new System.Windows.Forms.RadioButton();
            this.radioButton9 = new System.Windows.Forms.RadioButton();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.scaleGroupBox = new System.Windows.Forms.GroupBox();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.outerPanel = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.filterToCallersCalleesMenuItem = new System.Windows.Forms.MenuItem();
            this.filterMenuItem = new System.Windows.Forms.MenuItem();
            this.resetFilterMenuItem = new System.Windows.Forms.MenuItem();
            this.pruneContextMenuItem = new System.Windows.Forms.MenuItem();
            this.selectRecursiveMenuItem = new System.Windows.Forms.MenuItem();
            this.selectAllMenuItem = new System.Windows.Forms.MenuItem();
            this.copyContextMenuItem = new System.Windows.Forms.MenuItem();
            this.zoomToNodeMenuItem = new System.Windows.Forms.MenuItem();
            this.findInterestingNodesMenuItem = new System.Windows.Forms.MenuItem();
            this.findMenuItem = new System.Windows.Forms.MenuItem();
            this.findAgainMenuItem = new System.Windows.Forms.MenuItem();
            this.showWhoAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.showNewObjectsMenuItem = new System.Windows.Forms.MenuItem();
            this.showWhoAllocatedNewMenuItem = new System.Windows.Forms.MenuItem();
            this.showObjectsAllocatedBetween = new System.Windows.Forms.MenuItem();
            this.showWhoAllocatedObjectsBetweenMenuItem = new System.Windows.Forms.MenuItem();
            this.showInstancesMenuItem = new System.Windows.Forms.MenuItem();
            this.showHistogramMenuItem = new System.Windows.Forms.MenuItem();
            this.showReferencesMenuItem = new System.Windows.Forms.MenuItem();
            this.versionTimer = new System.Windows.Forms.Timer(this.components);
            this.mainMenu1 = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.selectAllMainMenuItem = new System.Windows.Forms.MenuItem();
            this.copyMainMenuItem = new System.Windows.Forms.MenuItem();
            this.findMainMenuItem = new System.Windows.Forms.MenuItem();
            this.findAgainMainMenuItem = new System.Windows.Forms.MenuItem();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.scaleGroupBox.SuspendLayout();
            this.outerPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Controls.Add(this.scaleGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1219, 80);
            this.panel1.TabIndex = 0;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.radioButton16);
            this.groupBox1.Controls.Add(this.radioButton15);
            this.groupBox1.Controls.Add(this.radioButton14);
            this.groupBox1.Controls.Add(this.radioButton13);
            this.groupBox1.Controls.Add(this.radioButton12);
            this.groupBox1.Controls.Add(this.radioButton11);
            this.groupBox1.Controls.Add(this.radioButton10);
            this.groupBox1.Controls.Add(this.radioButton9);
            this.groupBox1.Controls.Add(this.radioButton8);
            this.groupBox1.Location = new System.Drawing.Point(576, 16);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(604, 48);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Detail";
            // 
            // radioButton16
            // 
            this.radioButton16.Location = new System.Drawing.Point(489, 16);
            this.radioButton16.Name = "radioButton16";
            this.radioButton16.Size = new System.Drawing.Size(109, 24);
            this.radioButton16.TabIndex = 8;
            this.radioButton16.Text = "20 (coarse)";
            this.radioButton16.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton15
            // 
            this.radioButton15.Location = new System.Drawing.Point(434, 16);
            this.radioButton15.Name = "radioButton15";
            this.radioButton15.Size = new System.Drawing.Size(49, 24);
            this.radioButton15.TabIndex = 7;
            this.radioButton15.Text = "10";
            this.radioButton15.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton14
            // 
            this.radioButton14.Location = new System.Drawing.Point(387, 16);
            this.radioButton14.Name = "radioButton14";
            this.radioButton14.Size = new System.Drawing.Size(41, 24);
            this.radioButton14.TabIndex = 6;
            this.radioButton14.Text = "5";
            this.radioButton14.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton13
            // 
            this.radioButton13.Location = new System.Drawing.Point(349, 16);
            this.radioButton13.Name = "radioButton13";
            this.radioButton13.Size = new System.Drawing.Size(32, 24);
            this.radioButton13.TabIndex = 5;
            this.radioButton13.Text = "2";
            this.radioButton13.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton12
            // 
            this.radioButton12.Checked = true;
            this.radioButton12.Location = new System.Drawing.Point(305, 16);
            this.radioButton12.Name = "radioButton12";
            this.radioButton12.Size = new System.Drawing.Size(38, 24);
            this.radioButton12.TabIndex = 4;
            this.radioButton12.TabStop = true;
            this.radioButton12.Text = "1";
            this.radioButton12.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton11
            // 
            this.radioButton11.Location = new System.Drawing.Point(247, 16);
            this.radioButton11.Name = "radioButton11";
            this.radioButton11.Size = new System.Drawing.Size(52, 24);
            this.radioButton11.TabIndex = 3;
            this.radioButton11.Text = "0.5";
            this.radioButton11.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton10
            // 
            this.radioButton10.Location = new System.Drawing.Point(191, 16);
            this.radioButton10.Name = "radioButton10";
            this.radioButton10.Size = new System.Drawing.Size(50, 24);
            this.radioButton10.TabIndex = 2;
            this.radioButton10.Text = "0.2";
            this.radioButton10.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton9
            // 
            this.radioButton9.Location = new System.Drawing.Point(132, 16);
            this.radioButton9.Name = "radioButton9";
            this.radioButton9.Size = new System.Drawing.Size(50, 24);
            this.radioButton9.TabIndex = 1;
            this.radioButton9.Text = "0.1";
            this.radioButton9.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // radioButton8
            // 
            this.radioButton8.Location = new System.Drawing.Point(8, 16);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(118, 24);
            this.radioButton8.TabIndex = 0;
            this.radioButton8.Text = "0 (everything)";
            this.radioButton8.CheckedChanged += new System.EventHandler(this.detailRadioButton_Click);
            // 
            // scaleGroupBox
            // 
            this.scaleGroupBox.Controls.Add(this.radioButton7);
            this.scaleGroupBox.Controls.Add(this.radioButton6);
            this.scaleGroupBox.Controls.Add(this.radioButton5);
            this.scaleGroupBox.Controls.Add(this.radioButton4);
            this.scaleGroupBox.Controls.Add(this.radioButton3);
            this.scaleGroupBox.Controls.Add(this.radioButton2);
            this.scaleGroupBox.Controls.Add(this.radioButton1);
            this.scaleGroupBox.Location = new System.Drawing.Point(16, 16);
            this.scaleGroupBox.Name = "scaleGroupBox";
            this.scaleGroupBox.Size = new System.Drawing.Size(540, 48);
            this.scaleGroupBox.TabIndex = 1;
            this.scaleGroupBox.TabStop = false;
            this.scaleGroupBox.Text = "Scale";
            // 
            // radioButton7
            // 
            this.radioButton7.Location = new System.Drawing.Point(402, 16);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(108, 24);
            this.radioButton7.TabIndex = 6;
            this.radioButton7.Text = "1000 (huge)";
            this.radioButton7.CheckedChanged += new System.EventHandler(this.scaleRadioButton_Click);
            // 
            // radioButton6
            // 
            this.radioButton6.Location = new System.Drawing.Point(336, 16);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(60, 24);
            this.radioButton6.TabIndex = 5;
            this.radioButton6.Text = "500";
            this.radioButton6.CheckedChanged += new System.EventHandler(this.scaleRadioButton_Click);
            // 
            // radioButton5
            // 
            this.radioButton5.Location = new System.Drawing.Point(277, 16);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(53, 24);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.Text = "200";
            this.radioButton5.CheckedChanged += new System.EventHandler(this.scaleRadioButton_Click);
            // 
            // radioButton4
            // 
            this.radioButton4.Checked = true;
            this.radioButton4.Location = new System.Drawing.Point(218, 16);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(53, 24);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.TabStop = true;
            this.radioButton4.Text = "100";
            this.radioButton4.CheckedChanged += new System.EventHandler(this.scaleRadioButton_Click);
            // 
            // radioButton3
            // 
            this.radioButton3.Location = new System.Drawing.Point(162, 16);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(50, 24);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "50";
            this.radioButton3.CheckedChanged += new System.EventHandler(this.scaleRadioButton_Click);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(107, 16);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(49, 24);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "20";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.scaleRadioButton_Click);
            // 
            // radioButton1
            // 
            this.radioButton1.Location = new System.Drawing.Point(16, 16);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(85, 24);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.Text = "10 (tiny)";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.scaleRadioButton_Click);
            // 
            // outerPanel
            // 
            this.outerPanel.BackColor = System.Drawing.Color.White;
            this.outerPanel.Controls.Add(this.graphPanel);
            this.outerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outerPanel.Location = new System.Drawing.Point(0, 80);
            this.outerPanel.Name = "outerPanel";
            this.outerPanel.Size = new System.Drawing.Size(1219, 575);
            this.outerPanel.TabIndex = 1;
            // 
            // graphPanel
            // 
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(864, 528);
            this.graphPanel.TabIndex = 0;
            this.graphPanel.DoubleClick += new System.EventHandler(this.graphPanel_DoubleClick);
            this.graphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseDown);
            this.graphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseMove);
            this.graphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPanel_Paint);
            // 
            // contextMenu
            // 
            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.filterToCallersCalleesMenuItem,
            this.filterMenuItem,
            this.resetFilterMenuItem,
            this.pruneContextMenuItem,
            this.selectRecursiveMenuItem,
            this.selectAllMenuItem,
            this.copyContextMenuItem,
            this.zoomToNodeMenuItem,
            this.findInterestingNodesMenuItem,
            this.findMenuItem,
            this.findAgainMenuItem,
            this.showWhoAllocatedMenuItem,
            this.showNewObjectsMenuItem,
            this.showWhoAllocatedNewMenuItem,
            this.showObjectsAllocatedBetween,
            this.showWhoAllocatedObjectsBetweenMenuItem,
            this.showInstancesMenuItem,
            this.showHistogramMenuItem,
            this.showReferencesMenuItem});
            // 
            // filterToCallersCalleesMenuItem
            // 
            this.filterToCallersCalleesMenuItem.Index = 0;
            this.filterToCallersCalleesMenuItem.Text = "Filter to callers && callees";
            this.filterToCallersCalleesMenuItem.Click += new System.EventHandler(this.filterToCallersCalleesMenuItem_Click);
            // 
            // filterMenuItem
            // 
            this.filterMenuItem.Index = 1;
            this.filterMenuItem.Text = "Filter...";
            this.filterMenuItem.Click += new System.EventHandler(this.filterMenuItem_Click);
            // 
            // resetFilterMenuItem
            // 
            this.resetFilterMenuItem.Index = 2;
            this.resetFilterMenuItem.Text = "Reset Filter";
            this.resetFilterMenuItem.Click += new System.EventHandler(this.resetFilterMenuItem_Click);
            // 
            // pruneContextMenuItem
            // 
            this.pruneContextMenuItem.Index = 3;
            this.pruneContextMenuItem.Text = "Prune to callers && callees";
            this.pruneContextMenuItem.Click += new System.EventHandler(this.pruneMenuItem_Click);
            // 
            // selectRecursiveMenuItem
            // 
            this.selectRecursiveMenuItem.Index = 4;
            this.selectRecursiveMenuItem.Text = "Select callers && callees";
            this.selectRecursiveMenuItem.Click += new System.EventHandler(this.selectRecursiveMenuItem_Click);
            // 
            // selectAllMenuItem
            // 
            this.selectAllMenuItem.Index = 5;
            this.selectAllMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
            this.selectAllMenuItem.Text = "Select All";
            this.selectAllMenuItem.Click += new System.EventHandler(this.selectAllMenuItem_Click);
            // 
            // copyContextMenuItem
            // 
            this.copyContextMenuItem.Index = 6;
            this.copyContextMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.copyContextMenuItem.Text = "Copy as text to clipboard";
            this.copyContextMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // zoomToNodeMenuItem
            // 
            this.zoomToNodeMenuItem.Index = 7;
            this.zoomToNodeMenuItem.Text = "Zoom to Node";
            this.zoomToNodeMenuItem.Click += new System.EventHandler(this.zoomToNodeMenuItem_Click);
            // 
            // findInterestingNodesMenuItem
            // 
            this.findInterestingNodesMenuItem.Index = 8;
            this.findInterestingNodesMenuItem.Text = "Find interesting nodes";
            this.findInterestingNodesMenuItem.Click += new System.EventHandler(this.findInterestingNodesMenuItem_Click);
            // 
            // findMenuItem
            // 
            this.findMenuItem.Index = 9;
            this.findMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlF;
            this.findMenuItem.Text = "Find routine...";
            this.findMenuItem.Click += new System.EventHandler(this.findMenuItem_Click);
            // 
            // findAgainMenuItem
            // 
            this.findAgainMenuItem.Index = 10;
            this.findAgainMenuItem.Shortcut = System.Windows.Forms.Shortcut.F3;
            this.findAgainMenuItem.Text = "Find Again";
            this.findAgainMenuItem.Click += new System.EventHandler(this.findAgainMenuItem_Click);
            // 
            // showWhoAllocatedMenuItem
            // 
            this.showWhoAllocatedMenuItem.Index = 11;
            this.showWhoAllocatedMenuItem.Text = "Show Who Allocated";
            this.showWhoAllocatedMenuItem.Click += new System.EventHandler(this.showWhoAllocatedMenuItem_Click);
            // 
            // showNewObjectsMenuItem
            // 
            this.showNewObjectsMenuItem.Index = 12;
            this.showNewObjectsMenuItem.Text = "Show New Objects";
            this.showNewObjectsMenuItem.Click += new System.EventHandler(this.showNewObjectsMenuItem_Click);
            // 
            // showWhoAllocatedNewMenuItem
            // 
            this.showWhoAllocatedNewMenuItem.Index = 13;
            this.showWhoAllocatedNewMenuItem.Text = "Show Who Allocated New Objects";
            this.showWhoAllocatedNewMenuItem.Click += new System.EventHandler(this.showWhoAllocatedNewMenuItem_Click);
            // 
            // showObjectsAllocatedBetween
            // 
            this.showObjectsAllocatedBetween.Index = 14;
            this.showObjectsAllocatedBetween.Text = "Show Objects Allocated between...";
            this.showObjectsAllocatedBetween.Click += new System.EventHandler(this.showObjectsAllocatedBetween_Click);
            // 
            // showWhoAllocatedObjectsBetweenMenuItem
            // 
            this.showWhoAllocatedObjectsBetweenMenuItem.Index = 15;
            this.showWhoAllocatedObjectsBetweenMenuItem.Text = "Show Who Allocated Objects between...";
            this.showWhoAllocatedObjectsBetweenMenuItem.Click += new System.EventHandler(this.showWhoAllocatedObjectsBetweenMenuItem_Click);
            // 
            // showInstancesMenuItem
            // 
            this.showInstancesMenuItem.Index = 16;
            this.showInstancesMenuItem.Text = "Show Individual Instances";
            this.showInstancesMenuItem.Click += new System.EventHandler(this.showInstancesMenuItem_Click);
            // 
            // showHistogramMenuItem
            // 
            this.showHistogramMenuItem.Index = 17;
            this.showHistogramMenuItem.Text = "Show Histogram";
            this.showHistogramMenuItem.Click += new System.EventHandler(this.showHistogramMenuItem_Click);
            // 
            // showReferencesMenuItem
            // 
            this.showReferencesMenuItem.Index = 18;
            this.showReferencesMenuItem.Text = "Show References";
            this.showReferencesMenuItem.Click += new System.EventHandler(this.showReferencesMenuItem_Click);
            // 
            // versionTimer
            // 
            this.versionTimer.Enabled = true;
            this.versionTimer.Tick += new System.EventHandler(this.versionTimer_Tick);
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
            this.selectAllMainMenuItem,
            this.copyMainMenuItem,
            this.findMainMenuItem,
            this.findAgainMainMenuItem});
            this.menuItem1.Text = "Edit";
            // 
            // selectAllMainMenuItem
            // 
            this.selectAllMainMenuItem.Index = 0;
            this.selectAllMainMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlA;
            this.selectAllMainMenuItem.Text = "Select All";
            this.selectAllMainMenuItem.Click += new System.EventHandler(this.selectAllMenuItem_Click);
            // 
            // copyMainMenuItem
            // 
            this.copyMainMenuItem.Index = 1;
            this.copyMainMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlC;
            this.copyMainMenuItem.Text = "Copy as text to clipboard";
            this.copyMainMenuItem.Click += new System.EventHandler(this.copyMenuItem_Click);
            // 
            // findMainMenuItem
            // 
            this.findMainMenuItem.Index = 2;
            this.findMainMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlF;
            this.findMainMenuItem.Text = "Find routine...";
            this.findMainMenuItem.Click += new System.EventHandler(this.findMenuItem_Click);
            // 
            // findAgainMainMenuItem
            // 
            this.findAgainMainMenuItem.Index = 3;
            this.findAgainMainMenuItem.Shortcut = System.Windows.Forms.Shortcut.F3;
            this.findAgainMainMenuItem.Text = "Find Again";
            this.findAgainMainMenuItem.Click += new System.EventHandler(this.findAgainMenuItem_Click);
            // 
            // GraphViewForm
            // 
            this.ClientSize = new System.Drawing.Size(1219, 655);
            this.Controls.Add(this.outerPanel);
            this.Controls.Add(this.panel1);
            this.Menu = this.mainMenu1;
            this.Name = "GraphViewForm";
            this.Text = "GraphViewForm";
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.scaleGroupBox.ResumeLayout(false);
            this.outerPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
	}
}

