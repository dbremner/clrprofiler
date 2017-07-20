using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.IO;
namespace CLRProfiler
{
	public sealed partial class AgeHistogram : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.GroupBox verticalScaleGroupBox;
        private System.Windows.Forms.Panel graphOuterPanel;
        private System.Windows.Forms.Panel typeLegendOuterPanel;
        private System.Windows.Forms.Panel graphPanel;
        private System.Windows.Forms.Panel typeLegendPanel;
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
        private System.Windows.Forms.RadioButton radioButton11;
        private System.Windows.Forms.RadioButton radioButton12;
        private System.Windows.Forms.RadioButton radioButton13;
        private System.Windows.Forms.RadioButton radioButton14;
        private System.Windows.Forms.RadioButton radioButton15;
        private System.Windows.Forms.RadioButton radioButton16;
        private System.Windows.Forms.RadioButton radioButton17;
        private System.Windows.Forms.RadioButton radioButton18;
        private System.Windows.Forms.RadioButton radioButton19;
        private System.Windows.Forms.RadioButton radioButton20;
        private System.Windows.Forms.GroupBox timeScaleGroupBox;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem showWhoAllocatedMenuItem;
        private System.Windows.Forms.MenuItem exportDataMenuItem;
        private System.Windows.Forms.SaveFileDialog exportSaveFileDialog;
        private System.Windows.Forms.Timer versionTimer;
        private System.Windows.Forms.RadioButton markersRadioButton;

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
            this.components = new System.ComponentModel.Container();
            this.panel1 = new System.Windows.Forms.Panel();
            this.timeScaleGroupBox = new System.Windows.Forms.GroupBox();
            this.markersRadioButton = new System.Windows.Forms.RadioButton();
            this.radioButton20 = new System.Windows.Forms.RadioButton();
            this.radioButton19 = new System.Windows.Forms.RadioButton();
            this.radioButton18 = new System.Windows.Forms.RadioButton();
            this.radioButton17 = new System.Windows.Forms.RadioButton();
            this.radioButton16 = new System.Windows.Forms.RadioButton();
            this.radioButton15 = new System.Windows.Forms.RadioButton();
            this.radioButton14 = new System.Windows.Forms.RadioButton();
            this.radioButton13 = new System.Windows.Forms.RadioButton();
            this.radioButton12 = new System.Windows.Forms.RadioButton();
            this.radioButton11 = new System.Windows.Forms.RadioButton();
            this.verticalScaleGroupBox = new System.Windows.Forms.GroupBox();
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
            this.graphOuterPanel = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.typeLegendOuterPanel = new System.Windows.Forms.Panel();
            this.typeLegendPanel = new System.Windows.Forms.Panel();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.showWhoAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.exportDataMenuItem = new System.Windows.Forms.MenuItem();
            this.exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.versionTimer = new System.Windows.Forms.Timer(this.components);
            this.panel1.SuspendLayout();
            this.timeScaleGroupBox.SuspendLayout();
            this.verticalScaleGroupBox.SuspendLayout();
            this.graphOuterPanel.SuspendLayout();
            this.typeLegendOuterPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.timeScaleGroupBox);
            this.panel1.Controls.Add(this.verticalScaleGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1308, 100);
            this.panel1.TabIndex = 0;
            // 
            // timeScaleGroupBox
            // 
            this.timeScaleGroupBox.Controls.Add(this.markersRadioButton);
            this.timeScaleGroupBox.Controls.Add(this.radioButton20);
            this.timeScaleGroupBox.Controls.Add(this.radioButton19);
            this.timeScaleGroupBox.Controls.Add(this.radioButton18);
            this.timeScaleGroupBox.Controls.Add(this.radioButton17);
            this.timeScaleGroupBox.Controls.Add(this.radioButton16);
            this.timeScaleGroupBox.Controls.Add(this.radioButton15);
            this.timeScaleGroupBox.Controls.Add(this.radioButton14);
            this.timeScaleGroupBox.Controls.Add(this.radioButton13);
            this.timeScaleGroupBox.Controls.Add(this.radioButton12);
            this.timeScaleGroupBox.Controls.Add(this.radioButton11);
            this.timeScaleGroupBox.Location = new System.Drawing.Point(512, 16);
            this.timeScaleGroupBox.Name = "timeScaleGroupBox";
            this.timeScaleGroupBox.Size = new System.Drawing.Size(512, 64);
            this.timeScaleGroupBox.TabIndex = 1;
            this.timeScaleGroupBox.TabStop = false;
            this.timeScaleGroupBox.Text = "Time Scale: Seconds/Bar";
            // 
            // markersRadioButton
            // 
            this.markersRadioButton.Enabled = false;
            this.markersRadioButton.Location = new System.Drawing.Point(432, 17);
            this.markersRadioButton.Name = "markersRadioButton";
            this.markersRadioButton.Size = new System.Drawing.Size(75, 38);
            this.markersRadioButton.TabIndex = 11;
            this.markersRadioButton.Text = "Use Markers";
            this.markersRadioButton.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton20
            // 
            this.radioButton20.Location = new System.Drawing.Point(16, 24);
            this.radioButton20.Name = "radioButton20";
            this.radioButton20.Size = new System.Drawing.Size(48, 24);
            this.radioButton20.TabIndex = 10;
            this.radioButton20.Text = "0.05";
            this.radioButton20.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton19
            // 
            this.radioButton19.Location = new System.Drawing.Point(387, 24);
            this.radioButton19.Name = "radioButton19";
            this.radioButton19.Size = new System.Drawing.Size(40, 24);
            this.radioButton19.TabIndex = 9;
            this.radioButton19.Text = "50";
            this.radioButton19.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton18
            // 
            this.radioButton18.Location = new System.Drawing.Point(350, 24);
            this.radioButton18.Name = "radioButton18";
            this.radioButton18.Size = new System.Drawing.Size(40, 24);
            this.radioButton18.TabIndex = 8;
            this.radioButton18.Text = "20";
            this.radioButton18.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton17
            // 
            this.radioButton17.Location = new System.Drawing.Point(310, 24);
            this.radioButton17.Name = "radioButton17";
            this.radioButton17.Size = new System.Drawing.Size(40, 24);
            this.radioButton17.TabIndex = 7;
            this.radioButton17.Text = "10";
            this.radioButton17.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton16
            // 
            this.radioButton16.Location = new System.Drawing.Point(105, 24);
            this.radioButton16.Name = "radioButton16";
            this.radioButton16.Size = new System.Drawing.Size(42, 24);
            this.radioButton16.TabIndex = 6;
            this.radioButton16.Text = "0.2";
            this.radioButton16.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton15
            // 
            this.radioButton15.Location = new System.Drawing.Point(152, 24);
            this.radioButton15.Name = "radioButton15";
            this.radioButton15.Size = new System.Drawing.Size(42, 24);
            this.radioButton15.TabIndex = 5;
            this.radioButton15.Text = "0.5";
            this.radioButton15.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton14
            // 
            this.radioButton14.Location = new System.Drawing.Point(273, 24);
            this.radioButton14.Name = "radioButton14";
            this.radioButton14.Size = new System.Drawing.Size(32, 24);
            this.radioButton14.TabIndex = 4;
            this.radioButton14.Text = "5";
            this.radioButton14.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton13
            // 
            this.radioButton13.Location = new System.Drawing.Point(236, 24);
            this.radioButton13.Name = "radioButton13";
            this.radioButton13.Size = new System.Drawing.Size(32, 24);
            this.radioButton13.TabIndex = 3;
            this.radioButton13.Text = "2";
            this.radioButton13.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton12
            // 
            this.radioButton12.Location = new System.Drawing.Point(199, 24);
            this.radioButton12.Name = "radioButton12";
            this.radioButton12.Size = new System.Drawing.Size(32, 24);
            this.radioButton12.TabIndex = 2;
            this.radioButton12.Text = "1";
            this.radioButton12.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton11
            // 
            this.radioButton11.Location = new System.Drawing.Point(64, 24);
            this.radioButton11.Name = "radioButton11";
            this.radioButton11.Size = new System.Drawing.Size(46, 24);
            this.radioButton11.TabIndex = 1;
            this.radioButton11.Text = "0.1";
            this.radioButton11.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // verticalScaleGroupBox
            // 
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
            this.verticalScaleGroupBox.Location = new System.Drawing.Point(24, 16);
            this.verticalScaleGroupBox.Name = "verticalScaleGroupBox";
            this.verticalScaleGroupBox.Size = new System.Drawing.Size(464, 64);
            this.verticalScaleGroupBox.TabIndex = 0;
            this.verticalScaleGroupBox.TabStop = false;
            this.verticalScaleGroupBox.Text = "Vertical Scale: KB/Pixel";
            // 
            // radioButton10
            // 
            this.radioButton10.Location = new System.Drawing.Point(400, 24);
            this.radioButton10.Name = "radioButton10";
            this.radioButton10.Size = new System.Drawing.Size(56, 24);
            this.radioButton10.TabIndex = 9;
            this.radioButton10.Text = "1000";
            this.radioButton10.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton9
            // 
            this.radioButton9.Location = new System.Drawing.Point(352, 24);
            this.radioButton9.Name = "radioButton9";
            this.radioButton9.Size = new System.Drawing.Size(48, 24);
            this.radioButton9.TabIndex = 8;
            this.radioButton9.Text = "500";
            this.radioButton9.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton8
            // 
            this.radioButton8.Location = new System.Drawing.Point(304, 24);
            this.radioButton8.Name = "radioButton8";
            this.radioButton8.Size = new System.Drawing.Size(48, 24);
            this.radioButton8.TabIndex = 7;
            this.radioButton8.Text = "200";
            this.radioButton8.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton7
            // 
            this.radioButton7.Location = new System.Drawing.Point(216, 24);
            this.radioButton7.Name = "radioButton7";
            this.radioButton7.Size = new System.Drawing.Size(40, 24);
            this.radioButton7.TabIndex = 6;
            this.radioButton7.Text = "50";
            this.radioButton7.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton6
            // 
            this.radioButton6.Location = new System.Drawing.Point(136, 24);
            this.radioButton6.Name = "radioButton6";
            this.radioButton6.Size = new System.Drawing.Size(40, 24);
            this.radioButton6.TabIndex = 5;
            this.radioButton6.Text = "10";
            this.radioButton6.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton5
            // 
            this.radioButton5.Location = new System.Drawing.Point(256, 24);
            this.radioButton5.Name = "radioButton5";
            this.radioButton5.Size = new System.Drawing.Size(48, 24);
            this.radioButton5.TabIndex = 4;
            this.radioButton5.Text = "100";
            this.radioButton5.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton4
            // 
            this.radioButton4.Location = new System.Drawing.Point(176, 24);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(40, 24);
            this.radioButton4.TabIndex = 3;
            this.radioButton4.Text = "20";
            this.radioButton4.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton3
            // 
            this.radioButton3.Location = new System.Drawing.Point(96, 24);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(32, 24);
            this.radioButton3.TabIndex = 2;
            this.radioButton3.Text = "5";
            this.radioButton3.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(56, 24);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(32, 24);
            this.radioButton2.TabIndex = 1;
            this.radioButton2.Text = "2";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // radioButton1
            // 
            this.radioButton1.Location = new System.Drawing.Point(16, 24);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(32, 24);
            this.radioButton1.TabIndex = 0;
            this.radioButton1.Text = "1";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.CheckedChanged);
            // 
            // graphOuterPanel
            // 
            this.graphOuterPanel.AutoScroll = true;
            this.graphOuterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphOuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.graphOuterPanel.Controls.Add(this.graphPanel);
            this.graphOuterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphOuterPanel.Location = new System.Drawing.Point(0, 100);
            this.graphOuterPanel.Name = "graphOuterPanel";
            this.graphOuterPanel.Size = new System.Drawing.Size(912, 561);
            this.graphOuterPanel.TabIndex = 1;
            // 
            // graphPanel
            // 
            this.graphPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(488, 528);
            this.graphPanel.TabIndex = 0;
            this.graphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseDown);
            this.graphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseMove);
            this.graphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPanel_Paint);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(912, 100);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 561);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // typeLegendOuterPanel
            // 
            this.typeLegendOuterPanel.AutoScroll = true;
            this.typeLegendOuterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.typeLegendOuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.typeLegendOuterPanel.Controls.Add(this.typeLegendPanel);
            this.typeLegendOuterPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.typeLegendOuterPanel.Location = new System.Drawing.Point(916, 100);
            this.typeLegendOuterPanel.Name = "typeLegendOuterPanel";
            this.typeLegendOuterPanel.Size = new System.Drawing.Size(392, 561);
            this.typeLegendOuterPanel.TabIndex = 3;
            // 
            // typeLegendPanel
            // 
            this.typeLegendPanel.BackColor = System.Drawing.SystemColors.Control;
            this.typeLegendPanel.Location = new System.Drawing.Point(0, 0);
            this.typeLegendPanel.Name = "typeLegendPanel";
            this.typeLegendPanel.Size = new System.Drawing.Size(384, 504);
            this.typeLegendPanel.TabIndex = 0;
            this.typeLegendPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.typeLegendPanel_MouseDown);
            this.typeLegendPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.typeLegendPanel_Paint);
            // 
            // contextMenu
            // 
            this.contextMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.showWhoAllocatedMenuItem,
            this.exportDataMenuItem});
            // 
            // showWhoAllocatedMenuItem
            // 
            this.showWhoAllocatedMenuItem.Index = 0;
            this.showWhoAllocatedMenuItem.Text = "Show Who Allocated";
            this.showWhoAllocatedMenuItem.Click += new System.EventHandler(this.showWhoAllocatedMenuItem_Click);
            // 
            // exportDataMenuItem
            // 
            this.exportDataMenuItem.Index = 1;
            this.exportDataMenuItem.Text = "Export Data to File...";
            this.exportDataMenuItem.Click += new System.EventHandler(this.exportMenuItem_Click);
            // 
            // exportSaveFileDialog
            // 
            this.exportSaveFileDialog.FileName = "doc1";
            // 
            // versionTimer
            // 
            this.versionTimer.Enabled = true;
            this.versionTimer.Tick += new System.EventHandler(this.versionTimer_Tick);
            // 
            // AgeHistogram
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1308, 661);
            this.Controls.Add(this.graphOuterPanel);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.typeLegendOuterPanel);
            this.Controls.Add(this.panel1);
            this.Name = "AgeHistogram";
            this.Text = "Histogram by Age for Live Objects";
            this.panel1.ResumeLayout(false);
            this.timeScaleGroupBox.ResumeLayout(false);
            this.verticalScaleGroupBox.ResumeLayout(false);
            this.graphOuterPanel.ResumeLayout(false);
            this.typeLegendOuterPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion

        private ToolTip toolTip;
	}
}

