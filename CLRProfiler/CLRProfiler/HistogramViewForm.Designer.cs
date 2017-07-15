using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
namespace CLRProfiler
{
	public partial class HistogramViewForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.RadioButton radioButton1;
        private System.Windows.Forms.RadioButton radioButton2;
        private System.Windows.Forms.RadioButton radioButton3;
        private System.Windows.Forms.RadioButton radioButton4;
        private System.Windows.Forms.RadioButton radioButton5;
        private System.Windows.Forms.RadioButton radioButton6;
        private System.Windows.Forms.RadioButton radioButton7;
        private System.Windows.Forms.RadioButton radioButton8;
        private System.Windows.Forms.RadioButton radioButton9;
        private System.Windows.Forms.RadioButton radioButton10;
        private System.Windows.Forms.Panel graphPanel;
        private System.Windows.Forms.Panel typeLegendPanel;
        private System.Windows.Forms.GroupBox verticalScaleGroupBox;
        private System.Windows.Forms.GroupBox horizontalScaleGroupBox;
        private System.Windows.Forms.RadioButton coarseRadioButton;
        private System.Windows.Forms.RadioButton veryFineRadioButton;
        private System.Windows.Forms.RadioButton mediumRadioButton;
        private System.Windows.Forms.RadioButton fineRadioButton;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem exportMenuItem;
        private System.Windows.Forms.SaveFileDialog exportSaveFileDialog;
        private System.Windows.Forms.MenuItem showWhoAllocatedMenuItem;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.Splitter splitter1;
        private RadioButton radioButton13;
        private RadioButton radioButton12;
        private RadioButton radioButton11;
        private RadioButton radioButton14;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose( bool disposing )
        {
            versionTimer.Stop();
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.horizontalScaleGroupBox = new System.Windows.Forms.GroupBox();
            this.veryFineRadioButton = new System.Windows.Forms.RadioButton();
            this.fineRadioButton = new System.Windows.Forms.RadioButton();
            this.mediumRadioButton = new System.Windows.Forms.RadioButton();
            this.coarseRadioButton = new System.Windows.Forms.RadioButton();
            this.verticalScaleGroupBox = new System.Windows.Forms.GroupBox();
            this.radioButton14 = new System.Windows.Forms.RadioButton();
            this.radioButton13 = new System.Windows.Forms.RadioButton();
            this.radioButton12 = new System.Windows.Forms.RadioButton();
            this.radioButton11 = new System.Windows.Forms.RadioButton();
            this.radioButton10 = new System.Windows.Forms.RadioButton();
            this.radioButton9 = new System.Windows.Forms.RadioButton();
            this.radioButton8 = new System.Windows.Forms.RadioButton();
            this.radioButton7 = new System.Windows.Forms.RadioButton();
            this.radioButton6 = new System.Windows.Forms.RadioButton();
            this.radioButton5 = new System.Windows.Forms.RadioButton();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.panel2 = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.typeLegendPanel = new System.Windows.Forms.Panel();
            this.versionTimer = new System.Timers.Timer();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.showWhoAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.exportMenuItem = new System.Windows.Forms.MenuItem();
            this.exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.horizontalScaleGroupBox.SuspendLayout();
            this.verticalScaleGroupBox.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.horizontalScaleGroupBox);
            this.panel1.Controls.Add(this.verticalScaleGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1000, 72);
            this.panel1.TabIndex = 0;
            // 
            // horizontalScaleGroupBox
            // 
            this.horizontalScaleGroupBox.Controls.Add(this.veryFineRadioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fineRadioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.mediumRadioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.coarseRadioButton);
            this.horizontalScaleGroupBox.Location = new System.Drawing.Point(692, 8);
            this.horizontalScaleGroupBox.Name = "horizontalScaleGroupBox";
            this.horizontalScaleGroupBox.Size = new System.Drawing.Size(292, 48);
            this.horizontalScaleGroupBox.TabIndex = 1;
            this.horizontalScaleGroupBox.TabStop = false;
            this.horizontalScaleGroupBox.Text = "Horizontal Scale";
            // 
            // veryFineRadioButton
            // 
            this.veryFineRadioButton.Location = new System.Drawing.Point(212, 16);
            this.veryFineRadioButton.Name = "veryFineRadioButton";
            this.veryFineRadioButton.Size = new System.Drawing.Size(72, 24);
            this.veryFineRadioButton.TabIndex = 3;
            this.veryFineRadioButton.Text = "Very Fine";
            this.veryFineRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fineRadioButton
            // 
            this.fineRadioButton.Location = new System.Drawing.Point(158, 16);
            this.fineRadioButton.Name = "fineRadioButton";
            this.fineRadioButton.Size = new System.Drawing.Size(48, 24);
            this.fineRadioButton.TabIndex = 2;
            this.fineRadioButton.Text = "Fine";
            this.fineRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // mediumRadioButton
            // 
            this.mediumRadioButton.Location = new System.Drawing.Point(88, 16);
            this.mediumRadioButton.Name = "mediumRadioButton";
            this.mediumRadioButton.Size = new System.Drawing.Size(64, 24);
            this.mediumRadioButton.TabIndex = 1;
            this.mediumRadioButton.Text = "Medium";
            this.mediumRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // coarseRadioButton
            // 
            this.coarseRadioButton.Checked = true;
            this.coarseRadioButton.Location = new System.Drawing.Point(18, 16);
            this.coarseRadioButton.Name = "coarseRadioButton";
            this.coarseRadioButton.Size = new System.Drawing.Size(64, 24);
            this.coarseRadioButton.TabIndex = 0;
            this.coarseRadioButton.TabStop = true;
            this.coarseRadioButton.Text = "Coarse";
            this.coarseRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // verticalScaleGroupBox
            // 
            this.verticalScaleGroupBox.Controls.Add(this.radioButton14);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton13);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton12);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton11);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton10);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton9);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton8);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton7);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton6);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton5);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton4);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton3);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton2);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton1);
            this.verticalScaleGroupBox.Location = new System.Drawing.Point(16, 8);
            this.verticalScaleGroupBox.Name = "verticalScaleGroupBox";
            this.verticalScaleGroupBox.Size = new System.Drawing.Size(670, 48);
            this.verticalScaleGroupBox.TabIndex = 0;
            this.verticalScaleGroupBox.TabStop = false;
            this.verticalScaleGroupBox.Text = "Vertical Scale: Kilobytes/Pixel";
            // 
            // radioButton14
            // 
            this.radioButton14.Location = new System.Drawing.Point(601, 16);
            this.radioButton14.Name = "radioButton14";
            this.radioButton14.Size = new System.Drawing.Size(63, 24);
            this.radioButton14.TabIndex = 13;
            this.radioButton14.Text = "20000";
            this.radioButton14.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton13
            // 
            this.radioButton13.Location = new System.Drawing.Point(541, 16);
            this.radioButton13.Name = "radioButton13";
            this.radioButton13.Size = new System.Drawing.Size(63, 24);
            this.radioButton13.TabIndex = 12;
            this.radioButton13.Text = "10000";
            this.radioButton13.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton12
            // 
            this.radioButton12.Location = new System.Drawing.Point(487, 16);
            this.radioButton12.Name = "radioButton12";
            this.radioButton12.Size = new System.Drawing.Size(57, 24);
            this.radioButton12.TabIndex = 11;
            this.radioButton12.Text = "5000";
            this.radioButton12.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton11
            // 
            this.radioButton11.Location = new System.Drawing.Point(433, 16);
            this.radioButton11.Name = "radioButton11";
            this.radioButton11.Size = new System.Drawing.Size(55, 24);
            this.radioButton11.TabIndex = 10;
            this.radioButton11.Text = "2000";
            this.radioButton11.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton10
            // 
            this.radioButton10.Location = new System.Drawing.Point(376, 16);
            this.radioButton10.Name = "radioButton10";
            this.radioButton10.Size = new System.Drawing.Size(56, 24);
            this.radioButton10.TabIndex = 9;
            this.radioButton10.Text = "1000";
            this.radioButton10.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton9
            // 
            this.radioButton9.Location = new System.Drawing.Point(328, 16);
            this.radioButton9.Name = "radioButton9";
            this.radioButton9.Size = new System.Drawing.Size(48, 24);
            this.radioButton9.TabIndex = 8;
            this.radioButton9.Text = "500";
            this.radioButton9.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton8
            // 
            this.radioButton8.Location = new System.Drawing.Point(280, 16);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(48, 24);
            this.radioButton8.TabIndex = 7;
            this.radioButton8.Text = "200";
            this.radioButton8.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton7
            // 
            this.radioButton7.Location = new System.Drawing.Point(232, 16);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(48, 24);
            this.radioButton7.TabIndex = 6;
            this.radioButton7.Text = "100";
            this.radioButton7.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton6
            // 
            this.radioButton6.Location = new System.Drawing.Point(192, 16);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(40, 24);
            this.radioButton6.TabIndex = 5;
            this.radioButton6.Text = "50";
            this.radioButton6.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton5
            // 
            this.radioButton5.Location = new System.Drawing.Point(152, 16);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(40, 24);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.Text = "20";
            this.radioButton5.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton4
            // 
            this.radioButton4.Location = new System.Drawing.Point(112, 16);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(40, 24);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.Text = "10";
            this.radioButton4.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton3
            // 
            this.radioButton3.Location = new System.Drawing.Point(80, 16);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(32, 24);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "5";
            this.radioButton3.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(48, 16);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(32, 24);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "2";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton1
            // 
            this.radioButton1.Location = new System.Drawing.Point(16, 16);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(24, 24);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.Text = "1";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.graphPanel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 72);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(692, 533);
            this.panel2.TabIndex = 1;
            // 
            // graphPanel
            // 
            this.graphPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(496, 520);
            this.graphPanel.TabIndex = 0;
            this.graphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseDown);
            this.graphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseMove);
            this.graphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPanel_Paint);
            // 
            // panel3
            // 
            this.panel3.AutoScroll = true;
            this.panel3.BackColor = System.Drawing.SystemColors.Control;
            this.panel3.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel3.Controls.Add(this.typeLegendPanel);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel3.Location = new System.Drawing.Point(696, 72);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(304, 533);
            this.panel3.TabIndex = 3;
            // 
            // typeLegendPanel
            // 
            this.typeLegendPanel.BackColor = System.Drawing.SystemColors.Control;
            this.typeLegendPanel.Location = new System.Drawing.Point(0, 0);
            this.typeLegendPanel.Name = "typeLegendPanel";
            this.typeLegendPanel.Size = new System.Drawing.Size(262, 531);
            this.typeLegendPanel.TabIndex = 0;
            this.typeLegendPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.typeLegendPanel_MouseDown);
            this.typeLegendPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.typeLegendPanel_Paint);
            // 
            // versionTimer
            // 
            this.versionTimer.Enabled = true;
            this.versionTimer.SynchronizingObject = this;
            this.versionTimer.Elapsed += new System.Timers.ElapsedEventHandler(this.versionTimer_Elapsed);
            // 
            // contextMenu
            // 
            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showWhoAllocatedMenuItem,
            this.exportMenuItem});
            // 
            // showWhoAllocatedMenuItem
            // 
            this.showWhoAllocatedMenuItem.Index = 0;
            this.showWhoAllocatedMenuItem.Text = "Show Who Allocated";
            this.showWhoAllocatedMenuItem.Click += new System.EventHandler(this.showWhoAllocatedMenuItem_Click);
            // 
            // exportMenuItem
            // 
            this.exportMenuItem.Index = 1;
            this.exportMenuItem.Text = "Export Data to File...";
            this.exportMenuItem.Click += new System.EventHandler(this.exportMenuItem_Click);
            // 
            // exportSaveFileDialog
            // 
            this.exportSaveFileDialog.FileName = "doc1";
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(692, 72);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 533);
            this.splitter1.TabIndex = 4;
            this.splitter1.TabStop = false;
            // 
            // HistogramViewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1000, 605);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Name = "HistogramViewForm";
            this.Text = "HistogramViewForm";
            this.panel1.ResumeLayout(false);
            this.horizontalScaleGroupBox.ResumeLayout(false);
            this.verticalScaleGroupBox.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private ToolTip toolTip;
	}
}

