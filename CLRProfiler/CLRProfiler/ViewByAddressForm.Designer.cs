using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.IO;
using JetBrains.Annotations;

namespace CLRProfiler
{
	public sealed partial class ViewByAddressForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.RadioButton fourRadioButton;
        private System.Windows.Forms.RadioButton eightRadioButton;
        private System.Windows.Forms.RadioButton sixteenRadioButton;
        private System.Windows.Forms.RadioButton thirtytwoRadioButton;
        private System.Windows.Forms.RadioButton sixtyfourRadioButton;
        private System.Windows.Forms.RadioButton onetwoeightRadioButton;
        private System.Windows.Forms.RadioButton twofivesixRadioButton;
        private System.Windows.Forms.RadioButton fiveonetwoRadioButton;
        private System.Windows.Forms.RadioButton tentwentyfourRadioButton;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel outerGraphPanel;
        private System.Windows.Forms.Panel graphPanel;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel typeLegendPanel;
        private System.Windows.Forms.GroupBox bytesPerPixelgroupBox;
	    [NotNull] private System.Windows.Forms.GroupBox heapWidthGroupBox;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem showAllocatorsMenuItem;
        private System.Windows.Forms.MenuItem showHistogramMenuItem;
        private System.Windows.Forms.MenuItem exportMenuItem;
        private System.Windows.Forms.SaveFileDialog exportSaveFileDialog;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.RadioButton threetwoRadioButton;
        private System.Windows.Forms.RadioButton sixfourradioButton;
        private RadioButton radioButton1;
        private RadioButton radioButton2;
        private RadioButton radioButton4;
        private RadioButton radioButton3;

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
            this.bytesPerPixelgroupBox = new System.Windows.Forms.GroupBox();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.sixtyfourRadioButton = new System.Windows.Forms.RadioButton();
            this.thirtytwoRadioButton = new System.Windows.Forms.RadioButton();
            this.sixteenRadioButton = new System.Windows.Forms.RadioButton();
            this.eightRadioButton = new System.Windows.Forms.RadioButton();
            this.fourRadioButton = new System.Windows.Forms.RadioButton();
            this.heapWidthGroupBox = new System.Windows.Forms.GroupBox();
            this.sixfourradioButton = new System.Windows.Forms.RadioButton();
            this.threetwoRadioButton = new System.Windows.Forms.RadioButton();
            this.tentwentyfourRadioButton = new System.Windows.Forms.RadioButton();
            this.fiveonetwoRadioButton = new System.Windows.Forms.RadioButton();
            this.twofivesixRadioButton = new System.Windows.Forms.RadioButton();
            this.onetwoeightRadioButton = new System.Windows.Forms.RadioButton();
            this.panel1 = new System.Windows.Forms.Panel();
            this.outerGraphPanel = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.typeLegendPanel = new System.Windows.Forms.Panel();
            this.versionTimer = new System.Timers.Timer();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.showAllocatorsMenuItem = new System.Windows.Forms.MenuItem();
            this.showHistogramMenuItem = new System.Windows.Forms.MenuItem();
            this.exportMenuItem = new System.Windows.Forms.MenuItem();
            this.exportSaveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.bytesPerPixelgroupBox.SuspendLayout();
            this.heapWidthGroupBox.SuspendLayout();
            this.panel1.SuspendLayout();
            this.outerGraphPanel.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).BeginInit();
            this.SuspendLayout();
            // 
            // bytesPerPixelgroupBox
            // 
            this.bytesPerPixelgroupBox.Controls.Add(this.radioButton4);
            this.bytesPerPixelgroupBox.Controls.Add(this.radioButton3);
            this.bytesPerPixelgroupBox.Controls.Add(this.radioButton2);
            this.bytesPerPixelgroupBox.Controls.Add(this.radioButton1);
            this.bytesPerPixelgroupBox.Controls.Add(this.sixtyfourRadioButton);
            this.bytesPerPixelgroupBox.Controls.Add(this.thirtytwoRadioButton);
            this.bytesPerPixelgroupBox.Controls.Add(this.sixteenRadioButton);
            this.bytesPerPixelgroupBox.Controls.Add(this.eightRadioButton);
            this.bytesPerPixelgroupBox.Controls.Add(this.fourRadioButton);
            this.bytesPerPixelgroupBox.Location = new System.Drawing.Point(40, 24);
            this.bytesPerPixelgroupBox.Name = "bytesPerPixelgroupBox";
            this.bytesPerPixelgroupBox.Size = new System.Drawing.Size(428, 48);
            this.bytesPerPixelgroupBox.TabIndex = 1;
            this.bytesPerPixelgroupBox.TabStop = false;
            this.bytesPerPixelgroupBox.Text = "Bytes / Pixel";
            // 
            // radioButton4
            // 
            this.radioButton4.Location = new System.Drawing.Point(365, 16);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(55, 24);
            this.radioButton4.TabIndex = 8;
            this.radioButton4.Text = "1024";
            this.radioButton4.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton3
            // 
            this.radioButton3.Location = new System.Drawing.Point(313, 16);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(47, 24);
            this.radioButton3.TabIndex = 7;
            this.radioButton3.Text = "512";
            this.radioButton3.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(264, 16);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(44, 24);
            this.radioButton2.TabIndex = 6;
            this.radioButton2.Text = "256";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton1
            // 
            this.radioButton1.Location = new System.Drawing.Point(214, 16);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(45, 24);
            this.radioButton1.TabIndex = 5;
            this.radioButton1.Text = "128";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // sixtyfourRadioButton
            // 
            this.sixtyfourRadioButton.Location = new System.Drawing.Point(168, 16);
            this.sixtyfourRadioButton.Name = "sixtyfourRadioButton";
            this.sixtyfourRadioButton.Size = new System.Drawing.Size(40, 24);
            this.sixtyfourRadioButton.TabIndex = 4;
            this.sixtyfourRadioButton.Text = "64";
            this.sixtyfourRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // thirtytwoRadioButton
            // 
            this.thirtytwoRadioButton.Location = new System.Drawing.Point(128, 16);
            this.thirtytwoRadioButton.Name = "thirtytwoRadioButton";
            this.thirtytwoRadioButton.Size = new System.Drawing.Size(40, 24);
            this.thirtytwoRadioButton.TabIndex = 3;
            this.thirtytwoRadioButton.Text = "32";
            this.thirtytwoRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // sixteenRadioButton
            // 
            this.sixteenRadioButton.Location = new System.Drawing.Point(88, 16);
            this.sixteenRadioButton.Name = "sixteenRadioButton";
            this.sixteenRadioButton.Size = new System.Drawing.Size(40, 24);
            this.sixteenRadioButton.TabIndex = 2;
            this.sixteenRadioButton.Text = "16";
            this.sixteenRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // eightRadioButton
            // 
            this.eightRadioButton.Location = new System.Drawing.Point(48, 16);
            this.eightRadioButton.Name = "eightRadioButton";
            this.eightRadioButton.Size = new System.Drawing.Size(32, 24);
            this.eightRadioButton.TabIndex = 1;
            this.eightRadioButton.Text = "8";
            this.eightRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fourRadioButton
            // 
            this.fourRadioButton.Location = new System.Drawing.Point(8, 16);
            this.fourRadioButton.Name = "fourRadioButton";
            this.fourRadioButton.Size = new System.Drawing.Size(35, 24);
            this.fourRadioButton.TabIndex = 0;
            this.fourRadioButton.Text = "4";
            this.fourRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // heapWidthGroupBox
            // 
            this.heapWidthGroupBox.Controls.Add(this.sixfourradioButton);
            this.heapWidthGroupBox.Controls.Add(this.threetwoRadioButton);
            this.heapWidthGroupBox.Controls.Add(this.tentwentyfourRadioButton);
            this.heapWidthGroupBox.Controls.Add(this.fiveonetwoRadioButton);
            this.heapWidthGroupBox.Controls.Add(this.twofivesixRadioButton);
            this.heapWidthGroupBox.Controls.Add(this.onetwoeightRadioButton);
            this.heapWidthGroupBox.Location = new System.Drawing.Point(487, 24);
            this.heapWidthGroupBox.Name = "heapWidthGroupBox";
            this.heapWidthGroupBox.Size = new System.Drawing.Size(309, 48);
            this.heapWidthGroupBox.TabIndex = 2;
            this.heapWidthGroupBox.TabStop = false;
            this.heapWidthGroupBox.Text = "Width / Addressrange";
            // 
            // sixfourradioButton
            // 
            this.sixfourradioButton.Location = new System.Drawing.Point(56, 16);
            this.sixfourradioButton.Name = "sixfourradioButton";
            this.sixfourradioButton.Size = new System.Drawing.Size(40, 24);
            this.sixfourradioButton.TabIndex = 5;
            this.sixfourradioButton.Text = "64";
            this.sixfourradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // threetwoRadioButton
            // 
            this.threetwoRadioButton.Location = new System.Drawing.Point(16, 16);
            this.threetwoRadioButton.Name = "threetwoRadioButton";
            this.threetwoRadioButton.Size = new System.Drawing.Size(40, 24);
            this.threetwoRadioButton.TabIndex = 4;
            this.threetwoRadioButton.Text = "32";
            this.threetwoRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // tentwentyfourRadioButton
            // 
            this.tentwentyfourRadioButton.Location = new System.Drawing.Point(240, 16);
            this.tentwentyfourRadioButton.Name = "tentwentyfourRadioButton";
            this.tentwentyfourRadioButton.Size = new System.Drawing.Size(52, 24);
            this.tentwentyfourRadioButton.TabIndex = 3;
            this.tentwentyfourRadioButton.Text = "1024";
            this.tentwentyfourRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveonetwoRadioButton
            // 
            this.fiveonetwoRadioButton.Location = new System.Drawing.Point(192, 16);
            this.fiveonetwoRadioButton.Name = "fiveonetwoRadioButton";
            this.fiveonetwoRadioButton.Size = new System.Drawing.Size(50, 24);
            this.fiveonetwoRadioButton.TabIndex = 2;
            this.fiveonetwoRadioButton.Text = "512";
            this.fiveonetwoRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twofivesixRadioButton
            // 
            this.twofivesixRadioButton.Location = new System.Drawing.Point(144, 16);
            this.twofivesixRadioButton.Name = "twofivesixRadioButton";
            this.twofivesixRadioButton.Size = new System.Drawing.Size(48, 24);
            this.twofivesixRadioButton.TabIndex = 1;
            this.twofivesixRadioButton.Text = "256";
            this.twofivesixRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // onetwoeightRadioButton
            // 
            this.onetwoeightRadioButton.Location = new System.Drawing.Point(96, 16);
            this.onetwoeightRadioButton.Name = "onetwoeightRadioButton";
            this.onetwoeightRadioButton.Size = new System.Drawing.Size(48, 24);
            this.onetwoeightRadioButton.TabIndex = 0;
            this.onetwoeightRadioButton.Text = "128";
            this.onetwoeightRadioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.bytesPerPixelgroupBox);
            this.panel1.Controls.Add(this.heapWidthGroupBox);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(992, 100);
            this.panel1.TabIndex = 3;
            // 
            // outerGraphPanel
            // 
            this.outerGraphPanel.AutoScroll = true;
            this.outerGraphPanel.BackColor = System.Drawing.SystemColors.Control;
            this.outerGraphPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.outerGraphPanel.Controls.Add(this.graphPanel);
            this.outerGraphPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.outerGraphPanel.Location = new System.Drawing.Point(0, 100);
            this.outerGraphPanel.Name = "outerGraphPanel";
            this.outerGraphPanel.Size = new System.Drawing.Size(647, 489);
            this.outerGraphPanel.TabIndex = 4;
            // 
            // graphPanel
            // 
            this.graphPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(512, 480);
            this.graphPanel.TabIndex = 0;
            this.graphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseDown);
            this.graphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseMove);
            this.graphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPanel_Paint);
            this.graphPanel.MouseUp += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseUp);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(647, 100);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(4, 489);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.AutoScroll = true;
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.typeLegendPanel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel2.Location = new System.Drawing.Point(651, 100);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(341, 489);
            this.panel2.TabIndex = 6;
            // 
            // typeLegendPanel
            // 
            this.typeLegendPanel.BackColor = System.Drawing.SystemColors.Control;
            this.typeLegendPanel.Location = new System.Drawing.Point(0, 0);
            this.typeLegendPanel.Name = "typeLegendPanel";
            this.typeLegendPanel.Size = new System.Drawing.Size(336, 480);
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
            this.showAllocatorsMenuItem,
            this.showHistogramMenuItem,
            this.exportMenuItem});
            // 
            // showAllocatorsMenuItem
            // 
            this.showAllocatorsMenuItem.Index = 0;
            this.showAllocatorsMenuItem.Text = "Show Who Allocated";
            this.showAllocatorsMenuItem.Click += new System.EventHandler(this.showAllocatorsMenuItem_Click);
            // 
            // showHistogramMenuItem
            // 
            this.showHistogramMenuItem.Index = 1;
            this.showHistogramMenuItem.Text = "Show Histogram by Size";
            this.showHistogramMenuItem.Click += new System.EventHandler(this.showHistogramMenuItem_Click);
            // 
            // exportMenuItem
            // 
            this.exportMenuItem.Index = 2;
            this.exportMenuItem.Text = "Export Data to File...";
            this.exportMenuItem.Click += new System.EventHandler(this.exportMenuItem_Click);
            // 
            // exportSaveFileDialog
            // 
            this.exportSaveFileDialog.FileName = "doc1";
            // 
            // ViewByAddressForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(992, 589);
            this.Controls.Add(this.outerGraphPanel);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "ViewByAddressForm";
            this.Text = "View Objects by Address";
            this.bytesPerPixelgroupBox.ResumeLayout(false);
            this.heapWidthGroupBox.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.outerGraphPanel.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion

        private ToolTip toolTip;
	}
}

