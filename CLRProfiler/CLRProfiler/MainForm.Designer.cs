using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Globalization;
using System.Xml;
using Microsoft.Win32.SafeHandles;
namespace CLRProfiler
{
	public sealed partial class MainForm : System.Windows.Forms.Form
	{
        private System.Windows.Forms.MenuItem menuItem1;
        private System.Windows.Forms.MenuItem menuItem3;
        private System.Windows.Forms.MainMenu mainMenu;
        private System.Windows.Forms.MenuItem exitMenuItem;
        private System.ComponentModel.IContainer components;

        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.MenuItem menuItem5;
        private System.Windows.Forms.MenuItem fontMenuItem;
        private System.Windows.Forms.MenuItem logFileOpenMenuItem;
        private System.Windows.Forms.MenuItem profileApplicationMenuItem;
        private System.Windows.Forms.Button startApplicationButton;
        private System.Windows.Forms.CheckBox profilingActiveCheckBox;
        private System.Windows.Forms.Button killApplicationButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Timer checkProcessTimer;
        private System.Windows.Forms.MenuItem setCommandLineMenuItem;
        private System.Windows.Forms.FontDialog fontDialog;
        private System.Windows.Forms.Button showHeapButton;
        private System.Windows.Forms.MenuItem menuItem8;
        private System.Windows.Forms.MenuItem viewTimeLineMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.MenuItem saveAsMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramAllocatedMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramRelocatedMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramFinalizerMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramCriticalFinalizerMenuItem;
        private System.Windows.Forms.MenuItem viewHeapGraphMenuItem;
        private System.Windows.Forms.MenuItem viewCallGraphMenuItem;
        private System.Windows.Forms.MenuItem viewAllocationGraphMenuItem;
        private System.Windows.Forms.MenuItem viewObjectsByAddressMenuItem;
        private System.Windows.Forms.MenuItem viewHistogramByAgeMenuItem;
        private System.Windows.Forms.MenuItem profileASP_NETmenuItem;
        private System.Windows.Forms.MenuItem profileServiceMenuItem;
        private System.Windows.Forms.MenuItem viewFunctionGraphMenuItem;
        private System.Windows.Forms.MenuItem viewModuleGraphMenuItem;
        private System.Windows.Forms.MenuItem viewClassGraphMenuItem;
        private System.Windows.Forms.MenuItem viewCommentsMenuItem;
        private System.Windows.Forms.CheckBox allocationsCheckBox;
        private System.Windows.Forms.CheckBox callsCheckBox;
        private System.Windows.Forms.MenuItem viewCallTreeMenuItem;
        private System.Windows.Forms.MenuItem viewAssemblyGraphMenuItem;
        private System.Windows.Forms.MenuItem menuItem2;
        private System.Windows.Forms.MenuItem viewComparisonMenuItem;
        private System.Windows.Forms.MenuItem viewSummaryMenuItem;
        private Button attachProcessButton;
        private GroupBox groupBox2;
        private Label label2;
        private Button detachProcessButton;
        private ComboBox targetCLRVersioncomboBox;
        private Button startURLButton;
        private MenuItem startURLMenuItem;
        private MenuItem attachProcessMenuItem;
        private MenuItem detachProcessMenuItem;
        private Button startWindowsStoreAppButton;
        private MenuItem menuStartWindowsStoreApp;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                    components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.profilingActiveCheckBox = new System.Windows.Forms.CheckBox();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.callsCheckBox = new System.Windows.Forms.CheckBox();
            this.allocationsCheckBox = new System.Windows.Forms.CheckBox();
            this.logFileOpenMenuItem = new System.Windows.Forms.MenuItem();
            this.setCommandLineMenuItem = new System.Windows.Forms.MenuItem();
            this.profileApplicationMenuItem = new System.Windows.Forms.MenuItem();
            this.profileASP_NETmenuItem = new System.Windows.Forms.MenuItem();
            this.killApplicationButton = new System.Windows.Forms.Button();
            this.startApplicationButton = new System.Windows.Forms.Button();
            this.checkProcessTimer = new System.Windows.Forms.Timer(this.components);
            this.fontMenuItem = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.mainMenu = new System.Windows.Forms.MainMenu(this.components);
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.menuStartWindowsStoreApp = new System.Windows.Forms.MenuItem();
            this.startURLMenuItem = new System.Windows.Forms.MenuItem();
            this.attachProcessMenuItem = new System.Windows.Forms.MenuItem();
            this.detachProcessMenuItem = new System.Windows.Forms.MenuItem();
            this.profileServiceMenuItem = new System.Windows.Forms.MenuItem();
            this.saveAsMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem3 = new System.Windows.Forms.MenuItem();
            this.exitMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem5 = new System.Windows.Forms.MenuItem();
            this.menuItem8 = new System.Windows.Forms.MenuItem();
            this.viewSummaryMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramAllocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramRelocatedMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramFinalizerMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramCriticalFinalizerMenuItem = new System.Windows.Forms.MenuItem();
            this.viewObjectsByAddressMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHistogramByAgeMenuItem = new System.Windows.Forms.MenuItem();
            this.viewAllocationGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewAssemblyGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewFunctionGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewModuleGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewClassGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewHeapGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewCallGraphMenuItem = new System.Windows.Forms.MenuItem();
            this.viewTimeLineMenuItem = new System.Windows.Forms.MenuItem();
            this.viewCommentsMenuItem = new System.Windows.Forms.MenuItem();
            this.viewCallTreeMenuItem = new System.Windows.Forms.MenuItem();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.viewComparisonMenuItem = new System.Windows.Forms.MenuItem();
            this.showHeapButton = new System.Windows.Forms.Button();
            this.fontDialog = new System.Windows.Forms.FontDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.attachProcessButton = new System.Windows.Forms.Button();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.targetCLRVersioncomboBox = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.detachProcessButton = new System.Windows.Forms.Button();
            this.startURLButton = new System.Windows.Forms.Button();
            this.startWindowsStoreAppButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // profilingActiveCheckBox
            // 
            this.profilingActiveCheckBox.Checked = true;
            this.profilingActiveCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.profilingActiveCheckBox.Location = new System.Drawing.Point(198, 15);
            this.profilingActiveCheckBox.Name = "profilingActiveCheckBox";
            this.profilingActiveCheckBox.Size = new System.Drawing.Size(123, 24);
            this.profilingActiveCheckBox.TabIndex = 7;
            this.profilingActiveCheckBox.Text = "Profiling active";
            this.profilingActiveCheckBox.CheckedChanged += new System.EventHandler(this.profilingActiveCheckBox_CheckedChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.callsCheckBox);
            this.groupBox1.Controls.Add(this.allocationsCheckBox);
            this.groupBox1.Location = new System.Drawing.Point(193, 66);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(133, 57);
            this.groupBox1.TabIndex = 7;
            this.groupBox1.TabStop = false;
            // 
            // callsCheckBox
            // 
            this.callsCheckBox.Location = new System.Drawing.Point(16, 32);
            this.callsCheckBox.Name = "callsCheckBox";
            this.callsCheckBox.Size = new System.Drawing.Size(94, 18);
            this.callsCheckBox.TabIndex = 9;
            this.callsCheckBox.Text = "Calls";
            this.callsCheckBox.CheckedChanged += new System.EventHandler(this.callsCheckBox_CheckedChanged);
            // 
            // allocationsCheckBox
            // 
            this.allocationsCheckBox.Location = new System.Drawing.Point(16, 15);
            this.allocationsCheckBox.Name = "allocationsCheckBox";
            this.allocationsCheckBox.Size = new System.Drawing.Size(90, 20);
            this.allocationsCheckBox.TabIndex = 8;
            this.allocationsCheckBox.Text = "Allocations";
            this.allocationsCheckBox.CheckedChanged += new System.EventHandler(this.allocationsCheckBox_CheckedChanged);
            // 
            // logFileOpenMenuItem
            // 
            this.logFileOpenMenuItem.Index = 0;
            this.logFileOpenMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlO;
            this.logFileOpenMenuItem.Text = "Open Log File...";
            this.logFileOpenMenuItem.Click += new System.EventHandler(this.fileOpenMenuItem_Click);
            // 
            // setCommandLineMenuItem
            // 
            this.setCommandLineMenuItem.Index = 9;
            this.setCommandLineMenuItem.Text = "Set Parameters...";
            this.setCommandLineMenuItem.Click += new System.EventHandler(this.setCommandLineMenuItem_Click);
            // 
            // profileApplicationMenuItem
            // 
            this.profileApplicationMenuItem.Index = 1;
            this.profileApplicationMenuItem.Text = "Profile Application...";
            this.profileApplicationMenuItem.Click += new System.EventHandler(this.profileApplicationMenuItem_Click);
            // 
            // profileASP_NETmenuItem
            // 
            this.profileASP_NETmenuItem.Index = 6;
            this.profileASP_NETmenuItem.Text = "Profile ASP.NET";
            this.profileASP_NETmenuItem.Click += new System.EventHandler(this.profileASP_NETmenuItem_Click);
            // 
            // killApplicationButton
            // 
            this.killApplicationButton.Location = new System.Drawing.Point(15, 147);
            this.killApplicationButton.Name = "killApplicationButton";
            this.killApplicationButton.Size = new System.Drawing.Size(164, 24);
            this.killApplicationButton.TabIndex = 5;
            this.killApplicationButton.Text = "Kill Application";
            this.killApplicationButton.Click += new System.EventHandler(this.killApplicationButton_Click);
            // 
            // startApplicationButton
            // 
            this.startApplicationButton.Location = new System.Drawing.Point(15, 12);
            this.startApplicationButton.Name = "startApplicationButton";
            this.startApplicationButton.Size = new System.Drawing.Size(164, 24);
            this.startApplicationButton.TabIndex = 1;
            this.startApplicationButton.Text = "Start Desktop App...";
            this.startApplicationButton.Click += new System.EventHandler(this.startApplicationButton_Click);
            // 
            // checkProcessTimer
            // 
            this.checkProcessTimer.Enabled = true;
            this.checkProcessTimer.Tick += new System.EventHandler(this.checkProcessTimer_Tick);
            // 
            // fontMenuItem
            // 
            this.fontMenuItem.Index = 0;
            this.fontMenuItem.Text = "Font...";
            this.fontMenuItem.Click += new System.EventHandler(this.fontMenuItem_Click);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(201, 66);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(40, 16);
            this.label1.TabIndex = 6;
            this.label1.Text = "Profile:";
            // 
            // mainMenu
            // 
            this.mainMenu.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.menuItem1,
            this.menuItem5,
            this.menuItem8});
            // 
            // menuItem1
            // 
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.logFileOpenMenuItem,
            this.profileApplicationMenuItem,
            this.menuStartWindowsStoreApp,
            this.startURLMenuItem,
            this.attachProcessMenuItem,
            this.detachProcessMenuItem,
            this.profileASP_NETmenuItem,
            this.profileServiceMenuItem,
            this.saveAsMenuItem,
            this.setCommandLineMenuItem,
            this.menuItem3,
            this.exitMenuItem});
            this.menuItem1.Text = "File";
            // 
            // menuStartWindowsStoreApp
            // 
            this.menuStartWindowsStoreApp.Index = 2;
            this.menuStartWindowsStoreApp.Text = "Profile Windows Store Application...";
            this.menuStartWindowsStoreApp.Click += new System.EventHandler(this.menuStartWindowsStoreApp_Click);
            // 
            // startURLMenuItem
            // 
            this.startURLMenuItem.Index = 3;
            this.startURLMenuItem.Text = "Start URL...";
            this.startURLMenuItem.Click += new System.EventHandler(this.startURLMenuItem_Click);
            // 
            // attachProcessMenuItem
            // 
            this.attachProcessMenuItem.Index = 4;
            this.attachProcessMenuItem.Text = "Attach Process...";
            this.attachProcessMenuItem.Click += new System.EventHandler(this.attachProcessMenuItem_Click);
            // 
            // detachProcessMenuItem
            // 
            this.detachProcessMenuItem.Enabled = false;
            this.detachProcessMenuItem.Index = 5;
            this.detachProcessMenuItem.Text = "Detach Process";
            this.detachProcessMenuItem.Click += new System.EventHandler(this.detachProcessMenuItem_Click);
            // 
            // profileServiceMenuItem
            // 
            this.profileServiceMenuItem.Index = 7;
            this.profileServiceMenuItem.Text = "Profile Service...";
            this.profileServiceMenuItem.Click += new System.EventHandler(this.profileServiceMenuItem_Click);
            // 
            // saveAsMenuItem
            // 
            this.saveAsMenuItem.Enabled = false;
            this.saveAsMenuItem.Index = 8;
            this.saveAsMenuItem.Shortcut = System.Windows.Forms.Shortcut.CtrlS;
            this.saveAsMenuItem.Text = "Save Profile As...";
            this.saveAsMenuItem.Click += new System.EventHandler(this.saveAsMenuItem_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.Index = 10;
            this.menuItem3.Text = "-";
            // 
            // exitMenuItem
            // 
            this.exitMenuItem.Index = 11;
            this.exitMenuItem.Text = "Exit";
            this.exitMenuItem.Click += new System.EventHandler(this.exitMenuItem_Click);
            // 
            // menuItem5
            // 
            this.menuItem5.Index = 1;
            this.menuItem5.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.fontMenuItem});
            this.menuItem5.Text = "Edit";
            // 
            // menuItem8
            // 
            this.menuItem8.Index = 2;
            this.menuItem8.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.viewSummaryMenuItem,
            this.viewHistogramAllocatedMenuItem,
            this.viewHistogramRelocatedMenuItem,
            this.viewHistogramFinalizerMenuItem,
            this.viewHistogramCriticalFinalizerMenuItem,
            this.viewObjectsByAddressMenuItem,
            this.viewHistogramByAgeMenuItem,
            this.viewAllocationGraphMenuItem,
            this.viewAssemblyGraphMenuItem,
            this.viewFunctionGraphMenuItem,
            this.viewModuleGraphMenuItem,
            this.viewClassGraphMenuItem,
            this.viewHeapGraphMenuItem,
            this.viewCallGraphMenuItem,
            this.viewTimeLineMenuItem,
            this.viewCommentsMenuItem,
            this.viewCallTreeMenuItem,
            this.menuItem2,
            this.viewComparisonMenuItem});
            this.menuItem8.Text = "View";
            // 
            // viewSummaryMenuItem
            // 
            this.viewSummaryMenuItem.Index = 0;
            this.viewSummaryMenuItem.Text = "Summary";
            this.viewSummaryMenuItem.Click += new System.EventHandler(this.viewSummaryMenuItem_Click);
            // 
            // viewHistogramAllocatedMenuItem
            // 
            this.viewHistogramAllocatedMenuItem.Index = 1;
            this.viewHistogramAllocatedMenuItem.Text = "Histogram Allocated Types";
            this.viewHistogramAllocatedMenuItem.Click += new System.EventHandler(this.viewHistogram_Click);
            // 
            // viewHistogramRelocatedMenuItem
            // 
            this.viewHistogramRelocatedMenuItem.Index = 2;
            this.viewHistogramRelocatedMenuItem.Text = "Histogram Relocated Types";
            this.viewHistogramRelocatedMenuItem.Click += new System.EventHandler(this.viewHistogramRelocatedMenuItem_Click);
            // 
            // viewHistogramFinalizerMenuItem
            // 
            this.viewHistogramFinalizerMenuItem.Index = 3;
            this.viewHistogramFinalizerMenuItem.Text = "Histogram Finalized Types";
            this.viewHistogramFinalizerMenuItem.Click += new System.EventHandler(this.viewHistogramFinalizerMenuItem_Click);
            // 
            // viewHistogramCriticalFinalizerMenuItem
            // 
            this.viewHistogramCriticalFinalizerMenuItem.Index = 4;
            this.viewHistogramCriticalFinalizerMenuItem.Text = "Histogram Critical Finalized Types";
            this.viewHistogramCriticalFinalizerMenuItem.Click += new System.EventHandler(this.viewHistogramCriticalFinalizerMenuItem_Click);
            // 
            // viewObjectsByAddressMenuItem
            // 
            this.viewObjectsByAddressMenuItem.Index = 5;
            this.viewObjectsByAddressMenuItem.Text = "Objects by Address";
            this.viewObjectsByAddressMenuItem.Click += new System.EventHandler(this.viewByAddressMenuItem_Click);
            // 
            // viewHistogramByAgeMenuItem
            // 
            this.viewHistogramByAgeMenuItem.Index = 6;
            this.viewHistogramByAgeMenuItem.Text = "Histogram by Age";
            this.viewHistogramByAgeMenuItem.Click += new System.EventHandler(this.viewAgeHistogram_Click);
            // 
            // viewAllocationGraphMenuItem
            // 
            this.viewAllocationGraphMenuItem.Index = 7;
            this.viewAllocationGraphMenuItem.Text = "Allocation Graph";
            this.viewAllocationGraphMenuItem.Click += new System.EventHandler(this.viewAllocationGraphmenuItem_Click);
            // 
            // viewAssemblyGraphMenuItem
            // 
            this.viewAssemblyGraphMenuItem.Index = 8;
            this.viewAssemblyGraphMenuItem.Text = "Assembly Graph";
            this.viewAssemblyGraphMenuItem.Click += new System.EventHandler(this.viewAssemblyGraphmenuItem_Click);
            // 
            // viewFunctionGraphMenuItem
            // 
            this.viewFunctionGraphMenuItem.Index = 9;
            this.viewFunctionGraphMenuItem.Text = "Function Graph";
            this.viewFunctionGraphMenuItem.Click += new System.EventHandler(this.viewFunctionGraphMenuItem_Click);
            // 
            // viewModuleGraphMenuItem
            // 
            this.viewModuleGraphMenuItem.Index = 10;
            this.viewModuleGraphMenuItem.Text = "Module Graph";
            this.viewModuleGraphMenuItem.Click += new System.EventHandler(this.viewModuleGraphMenuItem_Click);
            // 
            // viewClassGraphMenuItem
            // 
            this.viewClassGraphMenuItem.Index = 11;
            this.viewClassGraphMenuItem.Text = "Class Graph";
            this.viewClassGraphMenuItem.Click += new System.EventHandler(this.viewClassGraphMenuItem_Click);
            // 
            // viewHeapGraphMenuItem
            // 
            this.viewHeapGraphMenuItem.Index = 12;
            this.viewHeapGraphMenuItem.Text = "Heap Graph";
            this.viewHeapGraphMenuItem.Click += new System.EventHandler(this.viewHeapGraphMenuItem_Click);
            // 
            // viewCallGraphMenuItem
            // 
            this.viewCallGraphMenuItem.Index = 13;
            this.viewCallGraphMenuItem.Text = "Call Graph";
            this.viewCallGraphMenuItem.Click += new System.EventHandler(this.viewCallGraphMenuItem_Click);
            // 
            // viewTimeLineMenuItem
            // 
            this.viewTimeLineMenuItem.Index = 14;
            this.viewTimeLineMenuItem.Text = "Time Line";
            this.viewTimeLineMenuItem.Click += new System.EventHandler(this.viewTimeLineMenuItem_Click);
            // 
            // viewCommentsMenuItem
            // 
            this.viewCommentsMenuItem.Index = 15;
            this.viewCommentsMenuItem.Text = "Comments";
            this.viewCommentsMenuItem.Click += new System.EventHandler(this.viewCommentsMenuItem_Click);
            // 
            // viewCallTreeMenuItem
            // 
            this.viewCallTreeMenuItem.Index = 16;
            this.viewCallTreeMenuItem.Text = "Call Tree";
            this.viewCallTreeMenuItem.Click += new System.EventHandler(this.viewCallTreeMenuItem_Click);
            // 
            // menuItem2
            // 
            this.menuItem2.Index = 17;
            this.menuItem2.Text = "-----------------------------";
            // 
            // viewComparisonMenuItem
            // 
            this.viewComparisonMenuItem.Index = 18;
            this.viewComparisonMenuItem.Text = "Comparison with ...";
            this.viewComparisonMenuItem.Click += new System.EventHandler(this.viewComparisonMenuItem_Click);
            // 
            // showHeapButton
            // 
            this.showHeapButton.Location = new System.Drawing.Point(15, 174);
            this.showHeapButton.Name = "showHeapButton";
            this.showHeapButton.Size = new System.Drawing.Size(164, 24);
            this.showHeapButton.TabIndex = 6;
            this.showHeapButton.Text = "Show Heap now";
            this.showHeapButton.Click += new System.EventHandler(this.showHeapButton_Click);
            // 
            // fontDialog
            // 
            this.fontDialog.Color = System.Drawing.SystemColors.ControlText;
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.FileName = "doc1";
            // 
            // attachProcessButton
            // 
            this.attachProcessButton.Location = new System.Drawing.Point(15, 93);
            this.attachProcessButton.Name = "attachProcessButton";
            this.attachProcessButton.Size = new System.Drawing.Size(164, 24);
            this.attachProcessButton.TabIndex = 3;
            this.attachProcessButton.Text = "Attach Process...";
            this.attachProcessButton.UseVisualStyleBackColor = true;
            this.attachProcessButton.Click += new System.EventHandler(this.attachProcessButton_Click);
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.targetCLRVersioncomboBox);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(194, 144);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(132, 54);
            this.groupBox2.TabIndex = 11;
            this.groupBox2.TabStop = false;
            // 
            // targetCLRVersioncomboBox
            // 
            this.targetCLRVersioncomboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.targetCLRVersioncomboBox.FormattingEnabled = true;
            this.targetCLRVersioncomboBox.Items.AddRange(new object[] {
            "V4 Desktop CLR",
            "V4 Core CLR",
            "V2 Desktop CLR"});
            this.targetCLRVersioncomboBox.Location = new System.Drawing.Point(7, 22);
            this.targetCLRVersioncomboBox.Name = "targetCLRVersioncomboBox";
            this.targetCLRVersioncomboBox.Size = new System.Drawing.Size(114, 21);
            this.targetCLRVersioncomboBox.TabIndex = 10;
            this.targetCLRVersioncomboBox.SelectedIndexChanged += new System.EventHandler(this.targetCLRVersioncomboBox_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(6, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 19);
            this.label2.TabIndex = 12;
            this.label2.Text = "Target CLR Version:";
            // 
            // detachProcessButton
            // 
            this.detachProcessButton.Enabled = false;
            this.detachProcessButton.Location = new System.Drawing.Point(15, 120);
            this.detachProcessButton.Name = "detachProcessButton";
            this.detachProcessButton.Size = new System.Drawing.Size(164, 24);
            this.detachProcessButton.TabIndex = 4;
            this.detachProcessButton.Text = "Detach Process";
            this.detachProcessButton.UseVisualStyleBackColor = true;
            this.detachProcessButton.Click += new System.EventHandler(this.detachProcessButton_Click);
            // 
            // startURLButton
            // 
            this.startURLButton.Location = new System.Drawing.Point(15, 66);
            this.startURLButton.Name = "startURLButton";
            this.startURLButton.Size = new System.Drawing.Size(164, 24);
            this.startURLButton.TabIndex = 2;
            this.startURLButton.Text = "Start URL...";
            this.startURLButton.Click += new System.EventHandler(this.startURLButton_Click);
            // 
            // startWindowsStoreAppButton
            // 
            this.startWindowsStoreAppButton.Location = new System.Drawing.Point(15, 39);
            this.startWindowsStoreAppButton.Name = "startWindowsStoreAppButton";
            this.startWindowsStoreAppButton.Size = new System.Drawing.Size(164, 24);
            this.startWindowsStoreAppButton.TabIndex = 12;
            this.startWindowsStoreAppButton.Text = "Start Windows Store App...";
            this.startWindowsStoreAppButton.Click += new System.EventHandler(this.startWindowsStoreAppButton_Click);
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(341, 208);
            this.Controls.Add(this.startWindowsStoreAppButton);
            this.Controls.Add(this.startURLButton);
            this.Controls.Add(this.detachProcessButton);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.attachProcessButton);
            this.Controls.Add(this.showHeapButton);
            this.Controls.Add(this.profilingActiveCheckBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.killApplicationButton);
            this.Controls.Add(this.startApplicationButton);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Menu = this.mainMenu;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "CLR Profiler";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.Form1_Closing);
            this.groupBox1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }
        #endregion
	}
}

