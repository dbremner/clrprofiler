using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using JetBrains.Annotations;

namespace CLRProfiler
{
	public sealed partial class TimeLineViewForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.RadioButton twoKradioButton;
        private System.Windows.Forms.RadioButton oneKradioButton;
        private System.Windows.Forms.RadioButton oneMSradioButton;
        private System.Windows.Forms.RadioButton twoMSradioButton;
        private System.Windows.Forms.RadioButton fiveMSradioButton;
        private System.Windows.Forms.RadioButton tenMSradioButton;
        private System.Windows.Forms.RadioButton twentyMSradioButton;
        private System.Windows.Forms.RadioButton fiftyMSradioButton;
        private System.Windows.Forms.RadioButton oneHundredMSradioButton;
        private System.Windows.Forms.RadioButton twoHundredMSradioButton;
        private System.Windows.Forms.RadioButton fiveHundredMSradioButton;
        private System.Windows.Forms.RadioButton oneThousandMSradioButton;
        private System.Windows.Forms.RadioButton oneHundredKradioButton;
        private System.Windows.Forms.RadioButton fiftyKradioButton;
        private System.Windows.Forms.RadioButton tenKradioButton;
        private System.Windows.Forms.RadioButton fiveKradioButton;
        private System.Windows.Forms.RadioButton twoHundredKradioButton;
        private System.Windows.Forms.RadioButton fiveHundredKradioButton;
        private System.Windows.Forms.Panel graphOuterPanel;
        private System.Windows.Forms.Panel legendOuterPanel;
	    [NotNull] private System.Windows.Forms.Panel graphPanel;
        private System.Windows.Forms.RadioButton twentyKradioButton;
        private System.Windows.Forms.GroupBox horizontalScaleGroupBox;
        private System.Windows.Forms.GroupBox verticalScaleGroupBox;
        private System.Windows.Forms.Panel typeLegendPanel;
        private System.Windows.Forms.MenuItem whoAllocatedMenuItem;
        private System.Windows.Forms.MenuItem showHistogramMenuItem;
        private System.Windows.Forms.MenuItem showObjectsMenuItem;
        private System.Windows.Forms.ContextMenu contextMenu;
        private System.Windows.Forms.MenuItem showRelocatedMenuItem;
        private System.Windows.Forms.MenuItem showAgeHistogramMenuItem;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.MenuItem setSelectionMenuItem;
        private System.Windows.Forms.MenuItem showTimeLineForSelectionMenuItem;
        private System.Windows.Forms.MenuItem showHeapGraphMenuItem;
        private RadioButton radioButton2;
        private RadioButton radioButton1;
        private RadioButton radioButton3;
        private RadioButton radioButton4;

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
            this.oneThousandMSradioButton = new System.Windows.Forms.RadioButton();
            this.fiveHundredMSradioButton = new System.Windows.Forms.RadioButton();
            this.twoHundredMSradioButton = new System.Windows.Forms.RadioButton();
            this.oneHundredMSradioButton = new System.Windows.Forms.RadioButton();
            this.fiftyMSradioButton = new System.Windows.Forms.RadioButton();
            this.twentyMSradioButton = new System.Windows.Forms.RadioButton();
            this.tenMSradioButton = new System.Windows.Forms.RadioButton();
            this.fiveMSradioButton = new System.Windows.Forms.RadioButton();
            this.twoMSradioButton = new System.Windows.Forms.RadioButton();
            this.oneMSradioButton = new System.Windows.Forms.RadioButton();
            this.verticalScaleGroupBox = new System.Windows.Forms.GroupBox();
            this.radioButton4 = new System.Windows.Forms.RadioButton();
            this.radioButton3 = new System.Windows.Forms.RadioButton();
            this.radioButton2 = new System.Windows.Forms.RadioButton();
            this.radioButton1 = new System.Windows.Forms.RadioButton();
            this.fiveHundredKradioButton = new System.Windows.Forms.RadioButton();
            this.twoHundredKradioButton = new System.Windows.Forms.RadioButton();
            this.oneHundredKradioButton = new System.Windows.Forms.RadioButton();
            this.fiftyKradioButton = new System.Windows.Forms.RadioButton();
            this.twentyKradioButton = new System.Windows.Forms.RadioButton();
            this.tenKradioButton = new System.Windows.Forms.RadioButton();
            this.fiveKradioButton = new System.Windows.Forms.RadioButton();
            this.twoKradioButton = new System.Windows.Forms.RadioButton();
            this.oneKradioButton = new System.Windows.Forms.RadioButton();
            this.graphOuterPanel = new System.Windows.Forms.Panel();
            this.graphPanel = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.legendOuterPanel = new System.Windows.Forms.Panel();
            this.typeLegendPanel = new System.Windows.Forms.Panel();
            this.versionTimer = new System.Timers.Timer();
            this.contextMenu = new System.Windows.Forms.ContextMenu();
            this.whoAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.showObjectsMenuItem = new System.Windows.Forms.MenuItem();
            this.showHistogramMenuItem = new System.Windows.Forms.MenuItem();
            this.showRelocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.showAgeHistogramMenuItem = new System.Windows.Forms.MenuItem();
            this.setSelectionMenuItem = new System.Windows.Forms.MenuItem();
            this.showTimeLineForSelectionMenuItem = new System.Windows.Forms.MenuItem();
            this.showHeapGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.panel1.SuspendLayout();
            this.horizontalScaleGroupBox.SuspendLayout();
            this.verticalScaleGroupBox.SuspendLayout();
            this.graphOuterPanel.SuspendLayout();
            this.legendOuterPanel.SuspendLayout();
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
            this.panel1.Size = new System.Drawing.Size(1072, 88);
            this.panel1.TabIndex = 0;
            // 
            // horizontalScaleGroupBox
            // 
            this.horizontalScaleGroupBox.Controls.Add(this.oneThousandMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fiveHundredMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.twoHundredMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.oneHundredMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fiftyMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.twentyMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.tenMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.fiveMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.twoMSradioButton);
            this.horizontalScaleGroupBox.Controls.Add(this.oneMSradioButton);
            this.horizontalScaleGroupBox.Location = new System.Drawing.Point(631, 8);
            this.horizontalScaleGroupBox.Name = "horizontalScaleGroupBox";
            this.horizontalScaleGroupBox.Size = new System.Drawing.Size(433, 64);
            this.horizontalScaleGroupBox.TabIndex = 1;
            this.horizontalScaleGroupBox.TabStop = false;
            this.horizontalScaleGroupBox.Text = "Horizontal Scale: Milliseconds/Pixel";
            // 
            // oneThousandMSradioButton
            // 
            this.oneThousandMSradioButton.Location = new System.Drawing.Point(370, 24);
            this.oneThousandMSradioButton.Name = "oneThousandMSradioButton";
            this.oneThousandMSradioButton.Size = new System.Drawing.Size(54, 24);
            this.oneThousandMSradioButton.TabIndex = 9;
            this.oneThousandMSradioButton.Text = "1000";
            this.oneThousandMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveHundredMSradioButton
            // 
            this.fiveHundredMSradioButton.Location = new System.Drawing.Point(320, 24);
            this.fiveHundredMSradioButton.Name = "fiveHundredMSradioButton";
            this.fiveHundredMSradioButton.Size = new System.Drawing.Size(48, 24);
            this.fiveHundredMSradioButton.TabIndex = 8;
            this.fiveHundredMSradioButton.Text = "500";
            this.fiveHundredMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoHundredMSradioButton
            // 
            this.twoHundredMSradioButton.Location = new System.Drawing.Point(272, 24);
            this.twoHundredMSradioButton.Name = "twoHundredMSradioButton";
            this.twoHundredMSradioButton.Size = new System.Drawing.Size(48, 24);
            this.twoHundredMSradioButton.TabIndex = 7;
            this.twoHundredMSradioButton.Text = "200";
            this.twoHundredMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneHundredMSradioButton
            // 
            this.oneHundredMSradioButton.Location = new System.Drawing.Point(224, 24);
            this.oneHundredMSradioButton.Name = "oneHundredMSradioButton";
            this.oneHundredMSradioButton.Size = new System.Drawing.Size(48, 24);
            this.oneHundredMSradioButton.TabIndex = 6;
            this.oneHundredMSradioButton.Text = "100";
            this.oneHundredMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiftyMSradioButton
            // 
            this.fiftyMSradioButton.Location = new System.Drawing.Point(184, 24);
            this.fiftyMSradioButton.Name = "fiftyMSradioButton";
            this.fiftyMSradioButton.Size = new System.Drawing.Size(40, 24);
            this.fiftyMSradioButton.TabIndex = 5;
            this.fiftyMSradioButton.Text = "50";
            this.fiftyMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twentyMSradioButton
            // 
            this.twentyMSradioButton.Location = new System.Drawing.Point(144, 24);
            this.twentyMSradioButton.Name = "twentyMSradioButton";
            this.twentyMSradioButton.Size = new System.Drawing.Size(40, 24);
            this.twentyMSradioButton.TabIndex = 4;
            this.twentyMSradioButton.Text = "20";
            this.twentyMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // tenMSradioButton
            // 
            this.tenMSradioButton.Location = new System.Drawing.Point(104, 24);
            this.tenMSradioButton.Name = "tenMSradioButton";
            this.tenMSradioButton.Size = new System.Drawing.Size(40, 24);
            this.tenMSradioButton.TabIndex = 3;
            this.tenMSradioButton.Text = "10";
            this.tenMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveMSradioButton
            // 
            this.fiveMSradioButton.Location = new System.Drawing.Point(72, 24);
            this.fiveMSradioButton.Name = "fiveMSradioButton";
            this.fiveMSradioButton.Size = new System.Drawing.Size(32, 24);
            this.fiveMSradioButton.TabIndex = 2;
            this.fiveMSradioButton.Text = "5";
            this.fiveMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoMSradioButton
            // 
            this.twoMSradioButton.Location = new System.Drawing.Point(40, 24);
            this.twoMSradioButton.Name = "twoMSradioButton";
            this.twoMSradioButton.Size = new System.Drawing.Size(32, 24);
            this.twoMSradioButton.TabIndex = 1;
            this.twoMSradioButton.Text = "2";
            this.twoMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneMSradioButton
            // 
            this.oneMSradioButton.Location = new System.Drawing.Point(8, 24);
            this.oneMSradioButton.Name = "oneMSradioButton";
            this.oneMSradioButton.Size = new System.Drawing.Size(24, 24);
            this.oneMSradioButton.TabIndex = 0;
            this.oneMSradioButton.Text = "1";
            this.oneMSradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // verticalScaleGroupBox
            // 
            this.verticalScaleGroupBox.Controls.Add(this.radioButton4);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton3);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton2);
            this.verticalScaleGroupBox.Controls.Add(this.radioButton1);
            this.verticalScaleGroupBox.Controls.Add(this.fiveHundredKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.twoHundredKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.oneHundredKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.fiftyKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.twentyKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.tenKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.fiveKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.twoKradioButton);
            this.verticalScaleGroupBox.Controls.Add(this.oneKradioButton);
            this.verticalScaleGroupBox.Location = new System.Drawing.Point(16, 8);
            this.verticalScaleGroupBox.Name = "verticalScaleGroupBox";
            this.verticalScaleGroupBox.Size = new System.Drawing.Size(609, 64);
            this.verticalScaleGroupBox.TabIndex = 0;
            this.verticalScaleGroupBox.TabStop = false;
            this.verticalScaleGroupBox.Text = "Vertical Scale: Kilobytes/Pixel";
            // 
            // radioButton4
            // 
            this.radioButton4.Location = new System.Drawing.Point(540, 24);
            this.radioButton4.Name = "radioButton4";
            this.radioButton4.Size = new System.Drawing.Size(63, 24);
            this.radioButton4.TabIndex = 12;
            this.radioButton4.Text = "10000";
            this.radioButton4.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton3
            // 
            this.radioButton3.Location = new System.Drawing.Point(486, 24);
            this.radioButton3.Name = "radioButton3";
            this.radioButton3.Size = new System.Drawing.Size(50, 24);
            this.radioButton3.TabIndex = 11;
            this.radioButton3.Text = "5000";
            this.radioButton3.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton2
            // 
            this.radioButton2.Location = new System.Drawing.Point(432, 24);
            this.radioButton2.Name = "radioButton2";
            this.radioButton2.Size = new System.Drawing.Size(56, 24);
            this.radioButton2.TabIndex = 10;
            this.radioButton2.Text = "2000";
            this.radioButton2.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // radioButton1
            // 
            this.radioButton1.Location = new System.Drawing.Point(374, 24);
            this.radioButton1.Name = "radioButton1";
            this.radioButton1.Size = new System.Drawing.Size(50, 24);
            this.radioButton1.TabIndex = 9;
            this.radioButton1.Text = "1000";
            this.radioButton1.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveHundredKradioButton
            // 
            this.fiveHundredKradioButton.Location = new System.Drawing.Point(320, 24);
            this.fiveHundredKradioButton.Name = "fiveHundredKradioButton";
            this.fiveHundredKradioButton.Size = new System.Drawing.Size(48, 24);
            this.fiveHundredKradioButton.TabIndex = 8;
            this.fiveHundredKradioButton.Text = "500";
            this.fiveHundredKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoHundredKradioButton
            // 
            this.twoHundredKradioButton.Location = new System.Drawing.Point(272, 24);
            this.twoHundredKradioButton.Name = "twoHundredKradioButton";
            this.twoHundredKradioButton.Size = new System.Drawing.Size(48, 24);
            this.twoHundredKradioButton.TabIndex = 7;
            this.twoHundredKradioButton.Text = "200";
            this.twoHundredKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneHundredKradioButton
            // 
            this.oneHundredKradioButton.Location = new System.Drawing.Point(224, 24);
            this.oneHundredKradioButton.Name = "oneHundredKradioButton";
            this.oneHundredKradioButton.Size = new System.Drawing.Size(48, 24);
            this.oneHundredKradioButton.TabIndex = 6;
            this.oneHundredKradioButton.Text = "100";
            this.oneHundredKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiftyKradioButton
            // 
            this.fiftyKradioButton.Location = new System.Drawing.Point(184, 24);
            this.fiftyKradioButton.Name = "fiftyKradioButton";
            this.fiftyKradioButton.Size = new System.Drawing.Size(40, 24);
            this.fiftyKradioButton.TabIndex = 5;
            this.fiftyKradioButton.Text = "50";
            this.fiftyKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twentyKradioButton
            // 
            this.twentyKradioButton.Location = new System.Drawing.Point(144, 24);
            this.twentyKradioButton.Name = "twentyKradioButton";
            this.twentyKradioButton.Size = new System.Drawing.Size(40, 24);
            this.twentyKradioButton.TabIndex = 4;
            this.twentyKradioButton.Text = "20";
            this.twentyKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // tenKradioButton
            // 
            this.tenKradioButton.Location = new System.Drawing.Point(104, 24);
            this.tenKradioButton.Name = "tenKradioButton";
            this.tenKradioButton.Size = new System.Drawing.Size(40, 24);
            this.tenKradioButton.TabIndex = 3;
            this.tenKradioButton.Text = "10";
            this.tenKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // fiveKradioButton
            // 
            this.fiveKradioButton.Location = new System.Drawing.Point(72, 24);
            this.fiveKradioButton.Name = "fiveKradioButton";
            this.fiveKradioButton.Size = new System.Drawing.Size(32, 24);
            this.fiveKradioButton.TabIndex = 2;
            this.fiveKradioButton.Text = "5";
            this.fiveKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // twoKradioButton
            // 
            this.twoKradioButton.Location = new System.Drawing.Point(40, 24);
            this.twoKradioButton.Name = "twoKradioButton";
            this.twoKradioButton.Size = new System.Drawing.Size(32, 24);
            this.twoKradioButton.TabIndex = 1;
            this.twoKradioButton.Text = "2";
            this.twoKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // oneKradioButton
            // 
            this.oneKradioButton.Location = new System.Drawing.Point(8, 24);
            this.oneKradioButton.Name = "oneKradioButton";
            this.oneKradioButton.Size = new System.Drawing.Size(32, 24);
            this.oneKradioButton.TabIndex = 0;
            this.oneKradioButton.Text = "1";
            this.oneKradioButton.CheckedChanged += new System.EventHandler(this.Refresh);
            // 
            // graphOuterPanel
            // 
            this.graphOuterPanel.AutoScroll = true;
            this.graphOuterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphOuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.graphOuterPanel.Controls.Add(this.graphPanel);
            this.graphOuterPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.graphOuterPanel.Location = new System.Drawing.Point(0, 88);
            this.graphOuterPanel.Name = "graphOuterPanel";
            this.graphOuterPanel.Size = new System.Drawing.Size(761, 577);
            this.graphOuterPanel.TabIndex = 1;
            this.graphOuterPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphOuterPanel_MouseDown);
            // 
            // graphPanel
            // 
            this.graphPanel.BackColor = System.Drawing.SystemColors.Control;
            this.graphPanel.Location = new System.Drawing.Point(0, 0);
            this.graphPanel.Name = "graphPanel";
            this.graphPanel.Size = new System.Drawing.Size(576, 480);
            this.graphPanel.TabIndex = 0;
            this.graphPanel.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseDown);
            this.graphPanel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.graphPanel_MouseMove);
            this.graphPanel.Paint += new System.Windows.Forms.PaintEventHandler(this.graphPanel_Paint);
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
            this.splitter1.Location = new System.Drawing.Point(761, 88);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 577);
            this.splitter1.TabIndex = 2;
            this.splitter1.TabStop = false;
            // 
            // legendOuterPanel
            // 
            this.legendOuterPanel.AutoScroll = true;
            this.legendOuterPanel.BackColor = System.Drawing.SystemColors.Control;
            this.legendOuterPanel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.legendOuterPanel.Controls.Add(this.typeLegendPanel);
            this.legendOuterPanel.Dock = System.Windows.Forms.DockStyle.Right;
            this.legendOuterPanel.Location = new System.Drawing.Point(764, 88);
            this.legendOuterPanel.Name = "legendOuterPanel";
            this.legendOuterPanel.Size = new System.Drawing.Size(308, 577);
            this.legendOuterPanel.TabIndex = 3;
            // 
            // typeLegendPanel
            // 
            this.typeLegendPanel.BackColor = System.Drawing.SystemColors.Control;
            this.typeLegendPanel.Location = new System.Drawing.Point(0, 0);
            this.typeLegendPanel.Name = "typeLegendPanel";
            this.typeLegendPanel.Size = new System.Drawing.Size(296, 480);
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
            this.whoAllocatedMenuItem,
            this.showObjectsMenuItem,
            this.showHistogramMenuItem,
            this.showRelocatedMenuItem,
            this.showAgeHistogramMenuItem,
            this.setSelectionMenuItem,
            this.showTimeLineForSelectionMenuItem,
            this.showHeapGraphMenuItem});
            // 
            // whoAllocatedMenuItem
            // 
            this.whoAllocatedMenuItem.Index = 0;
            this.whoAllocatedMenuItem.Text = "Show Who Allocated";
            this.whoAllocatedMenuItem.Click += new System.EventHandler(this.whoAllocatedMenuItem_Click);
            // 
            // showObjectsMenuItem
            // 
            this.showObjectsMenuItem.Index = 1;
            this.showObjectsMenuItem.Text = "Show Objects by Address";
            this.showObjectsMenuItem.Click += new System.EventHandler(this.showObjectsMenuItem_Click);
            // 
            // showHistogramMenuItem
            // 
            this.showHistogramMenuItem.Index = 2;
            this.showHistogramMenuItem.Text = "Show Histogram Allocated Types";
            this.showHistogramMenuItem.Click += new System.EventHandler(this.showHistogramMenuItem_Click);
            // 
            // showRelocatedMenuItem
            // 
            this.showRelocatedMenuItem.Index = 3;
            this.showRelocatedMenuItem.Text = "Show Histogram Relocated Types";
            this.showRelocatedMenuItem.Click += new System.EventHandler(this.showRelocatedMenuItem_Click);
            // 
            // showAgeHistogramMenuItem
            // 
            this.showAgeHistogramMenuItem.Index = 4;
            this.showAgeHistogramMenuItem.Text = "Show Histogram By Age";
            this.showAgeHistogramMenuItem.Click += new System.EventHandler(this.showAgeHistogramMenuItem_Click);
            // 
            // setSelectionMenuItem
            // 
            this.setSelectionMenuItem.Index = 5;
            this.setSelectionMenuItem.Text = "Set Selection to Marker...";
            this.setSelectionMenuItem.Click += new System.EventHandler(this.setSelectionMenuItem_Click);
            // 
            // showTimeLineForSelectionMenuItem
            // 
            this.showTimeLineForSelectionMenuItem.Index = 6;
            this.showTimeLineForSelectionMenuItem.Text = "Show Time Line for Selection";
            this.showTimeLineForSelectionMenuItem.Click += new System.EventHandler(this.showTimeLineForSelectionMenuItem_Click);
            // 
            // showHeapGraphMenuItem
            // 
            this.showHeapGraphMenuItem.Index = 7;
            this.showHeapGraphMenuItem.Text = "Show Heap Graph";
            this.showHeapGraphMenuItem.Click += new System.EventHandler(this.showHeapGraphMenuItem_Click);
            // 
            // TimeLineViewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1072, 665);
            this.Controls.Add(this.graphOuterPanel);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.legendOuterPanel);
            this.Controls.Add(this.panel1);
            this.Name = "TimeLineViewForm";
            this.Text = "Time Line";
            this.panel1.ResumeLayout(false);
            this.horizontalScaleGroupBox.ResumeLayout(false);
            this.verticalScaleGroupBox.ResumeLayout(false);
            this.graphOuterPanel.ResumeLayout(false);
            this.legendOuterPanel.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.versionTimer)).EndInit();
            this.ResumeLayout(false);

        }
        #endregion
        
        private ToolTip toolTip;
	}
}

