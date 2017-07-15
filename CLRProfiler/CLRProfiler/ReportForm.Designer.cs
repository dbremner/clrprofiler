using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Data;
using System.IO;
using System.Text;
namespace CLRProfiler
{
	public partial class ReportForm : System.Windows.Forms.Form
	{
		private DataViewManager dvm;
		private DataViewManager dvm_caller;
		private DataViewManager dvm_callee;
		private DataGridTableStyle styleBase = new DataGridTableStyle();
		private DataGridTableStyle styleCaller = new DataGridTableStyle();
		private DataGridTableStyle styleCallee = new DataGridTableStyle();
		private DataGridTableStyle styleSelected = new DataGridTableStyle();

		
		private System.Windows.Forms.TabPage tabpallocdiff;
		private System.Windows.Forms.DataGridTableStyle dataGridTableStyle1;
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.GroupBox gpboption;
		private System.Windows.Forms.TextBox txtbPrevlog;
		private System.Windows.Forms.TextBox txtbCurrlog;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Splitter splitter3;
		private System.Windows.Forms.Splitter splitter4;
		private System.Windows.Forms.DataGrid dgToplevelDiff;
		private System.Windows.Forms.DataGrid dgCallee;
		private System.Windows.Forms.DataGrid dgSelected;
		private System.Windows.Forms.DataGrid dgCaller;
		private System.Windows.Forms.RadioButton rdbDetail20;
		private System.Windows.Forms.RadioButton rdbDetail0;
		private System.Windows.Forms.RadioButton rdbDetail01;
		private System.Windows.Forms.RadioButton rdbDetail02;
		private System.Windows.Forms.RadioButton rdbDetail05;
		private System.Windows.Forms.RadioButton rdbDetail1;
		private System.Windows.Forms.RadioButton rdbDetail2;
		private System.Windows.Forms.RadioButton rdbDetail5;
		private System.Windows.Forms.RadioButton rdbDetail10;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnBrowse1;
		private System.Windows.Forms.Button btnRun;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button btnBrowse2;
		private System.Windows.Forms.Panel panel2;

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
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabpallocdiff = new System.Windows.Forms.TabPage();
			this.dgToplevelDiff = new System.Windows.Forms.DataGrid();
			this.splitter4 = new System.Windows.Forms.Splitter();
			this.dgCaller = new System.Windows.Forms.DataGrid();
			this.splitter3 = new System.Windows.Forms.Splitter();
			this.dgSelected = new System.Windows.Forms.DataGrid();
			this.splitter1 = new System.Windows.Forms.Splitter();
			this.panel1 = new System.Windows.Forms.Panel();
			this.btnRun = new System.Windows.Forms.Button();
			this.btnBrowse2 = new System.Windows.Forms.Button();
			this.btnBrowse1 = new System.Windows.Forms.Button();
			this.label2 = new System.Windows.Forms.Label();
			this.label1 = new System.Windows.Forms.Label();
			this.txtbPrevlog = new System.Windows.Forms.TextBox();
			this.txtbCurrlog = new System.Windows.Forms.TextBox();
			this.gpboption = new System.Windows.Forms.GroupBox();
			this.rdbDetail10 = new System.Windows.Forms.RadioButton();
			this.rdbDetail5 = new System.Windows.Forms.RadioButton();
			this.rdbDetail2 = new System.Windows.Forms.RadioButton();
			this.rdbDetail1 = new System.Windows.Forms.RadioButton();
			this.rdbDetail05 = new System.Windows.Forms.RadioButton();
			this.rdbDetail02 = new System.Windows.Forms.RadioButton();
			this.rdbDetail01 = new System.Windows.Forms.RadioButton();
			this.rdbDetail20 = new System.Windows.Forms.RadioButton();
			this.rdbDetail0 = new System.Windows.Forms.RadioButton();
			this.splitter2 = new System.Windows.Forms.Splitter();
			this.dgCallee = new System.Windows.Forms.DataGrid();
			this.dataGridTableStyle1 = new System.Windows.Forms.DataGridTableStyle();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.panel2 = new System.Windows.Forms.Panel();
			this.tabControl1.SuspendLayout();
			this.tabpallocdiff.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgToplevelDiff)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgCaller)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.dgSelected)).BeginInit();
			this.panel1.SuspendLayout();
			this.gpboption.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.dgCallee)).BeginInit();
			this.panel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabpallocdiff);
			this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tabControl1.ItemSize = new System.Drawing.Size(79, 18);
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(1216, 886);
			this.tabControl1.TabIndex = 1;
			// 
			// tabpallocdiff
			// 
			this.tabpallocdiff.Controls.Add(this.dgToplevelDiff);
			this.tabpallocdiff.Controls.Add(this.splitter4);
			this.tabpallocdiff.Controls.Add(this.dgCaller);
			this.tabpallocdiff.Controls.Add(this.splitter3);
			this.tabpallocdiff.Controls.Add(this.dgSelected);
			this.tabpallocdiff.Controls.Add(this.splitter1);
			this.tabpallocdiff.Controls.Add(this.panel1);
			this.tabpallocdiff.Controls.Add(this.splitter2);
			this.tabpallocdiff.Controls.Add(this.dgCallee);
			this.tabpallocdiff.Location = new System.Drawing.Point(4, 22);
			this.tabpallocdiff.Name = "tabpallocdiff";
			this.tabpallocdiff.Size = new System.Drawing.Size(1208, 860);
			this.tabpallocdiff.TabIndex = 0;
			this.tabpallocdiff.Text = "Allocation Diff";
			// 
			// dgToplevelDiff
			// 
			this.dgToplevelDiff.DataMember = "";
			this.dgToplevelDiff.Dock = System.Windows.Forms.DockStyle.Fill;
			this.dgToplevelDiff.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgToplevelDiff.Location = new System.Drawing.Point(0, 75);
			this.dgToplevelDiff.Name = "dgToplevelDiff";
			this.dgToplevelDiff.Size = new System.Drawing.Size(1208, 327);
			this.dgToplevelDiff.TabIndex = 15;
			this.dgToplevelDiff.DoubleClick += new System.EventHandler(this.dgToplevelDiff_DoubleClick);
			this.dgToplevelDiff.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgToplevelDiff_MouseMove);
			// 
			// splitter4
			// 
			this.splitter4.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter4.Location = new System.Drawing.Point(0, 402);
			this.splitter4.Name = "splitter4";
			this.splitter4.Size = new System.Drawing.Size(1208, 3);
			this.splitter4.TabIndex = 14;
			this.splitter4.TabStop = false;
			// 
			// dgCaller
			// 
			this.dgCaller.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgCaller.CaptionBackColor = System.Drawing.SystemColors.ActiveBorder;
			this.dgCaller.DataMember = "";
			this.dgCaller.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dgCaller.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgCaller.Location = new System.Drawing.Point(0, 405);
			this.dgCaller.Name = "dgCaller";
			this.dgCaller.Size = new System.Drawing.Size(1208, 120);
			this.dgCaller.TabIndex = 13;
			this.dgCaller.DoubleClick += new System.EventHandler(this.dgCaller_DoubleClick);
			this.dgCaller.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgCaller_MouseMove);
			// 
			// splitter3
			// 
			this.splitter3.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter3.Location = new System.Drawing.Point(0, 525);
			this.splitter3.Name = "splitter3";
			this.splitter3.Size = new System.Drawing.Size(1208, 8);
			this.splitter3.TabIndex = 12;
			this.splitter3.TabStop = false;
			// 
			// dgSelected
			// 
			this.dgSelected.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgSelected.CaptionBackColor = System.Drawing.SystemColors.ActiveBorder;
			this.dgSelected.DataMember = "";
			this.dgSelected.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dgSelected.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgSelected.Location = new System.Drawing.Point(0, 533);
			this.dgSelected.Name = "dgSelected";
			this.dgSelected.Size = new System.Drawing.Size(1208, 64);
			this.dgSelected.TabIndex = 11;
			this.dgSelected.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgSelected_MouseMove);
			// 
			// splitter1
			// 
			this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
			this.splitter1.Location = new System.Drawing.Point(0, 72);
			this.splitter1.Name = "splitter1";
			this.splitter1.Size = new System.Drawing.Size(1208, 3);
			this.splitter1.TabIndex = 10;
			this.splitter1.TabStop = false;
			// 
			// panel1
			// 
			this.panel1.Controls.Add(this.btnRun);
			this.panel1.Controls.Add(this.btnBrowse2);
			this.panel1.Controls.Add(this.btnBrowse1);
			this.panel1.Controls.Add(this.label2);
			this.panel1.Controls.Add(this.label1);
			this.panel1.Controls.Add(this.txtbPrevlog);
			this.panel1.Controls.Add(this.txtbCurrlog);
			this.panel1.Controls.Add(this.gpboption);
			this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
			this.panel1.Location = new System.Drawing.Point(0, 0);
			this.panel1.Name = "panel1";
			this.panel1.Size = new System.Drawing.Size(1208, 72);
			this.panel1.TabIndex = 9;
			// 
			// btnRun
			// 
			this.btnRun.Location = new System.Drawing.Point(1056, 32);
			this.btnRun.Name = "btnRun";
			this.btnRun.TabIndex = 10;
			this.btnRun.Text = "Run";
			this.btnRun.Click += new System.EventHandler(this.btnRun_Click);
			// 
			// btnBrowse2
			// 
			this.btnBrowse2.Location = new System.Drawing.Point(464, 48);
			this.btnBrowse2.Name = "btnBrowse2";
			this.btnBrowse2.Size = new System.Drawing.Size(56, 23);
			this.btnBrowse2.TabIndex = 9;
			this.btnBrowse2.Text = "Browse...";
			this.btnBrowse2.Click += new System.EventHandler(this.btnBrowse2_Click);
			// 
			// btnBrowse1
			// 
			this.btnBrowse1.Location = new System.Drawing.Point(464, 16);
			this.btnBrowse1.Name = "btnBrowse1";
			this.btnBrowse1.Size = new System.Drawing.Size(56, 23);
			this.btnBrowse1.TabIndex = 8;
			this.btnBrowse1.Text = "Browse...";
			this.btnBrowse1.Click += new System.EventHandler(this.btnBrowse1_Click);
			// 
			// label2
			// 
			this.label2.Location = new System.Drawing.Point(16, 48);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(72, 23);
			this.label2.TabIndex = 7;
			this.label2.Text = "New Logfile: ";
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(16, 16);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(72, 23);
			this.label1.TabIndex = 6;
			this.label1.Text = "Old Logfile: ";
			// 
			// txtbPrevlog
			// 
			this.txtbPrevlog.Location = new System.Drawing.Point(88, 16);
			this.txtbPrevlog.Name = "txtbPrevlog";
			this.txtbPrevlog.Size = new System.Drawing.Size(368, 20);
			this.txtbPrevlog.TabIndex = 2;
			this.txtbPrevlog.Text = "textBox1";
			// 
			// txtbCurrlog
			// 
			this.txtbCurrlog.Location = new System.Drawing.Point(88, 48);
			this.txtbCurrlog.Name = "txtbCurrlog";
			this.txtbCurrlog.Size = new System.Drawing.Size(368, 20);
			this.txtbCurrlog.TabIndex = 4;
			this.txtbCurrlog.Text = "textBox2";
			// 
			// gpboption
			// 
			this.gpboption.Controls.Add(this.rdbDetail10);
			this.gpboption.Controls.Add(this.rdbDetail5);
			this.gpboption.Controls.Add(this.rdbDetail2);
			this.gpboption.Controls.Add(this.rdbDetail1);
			this.gpboption.Controls.Add(this.rdbDetail05);
			this.gpboption.Controls.Add(this.rdbDetail02);
			this.gpboption.Controls.Add(this.rdbDetail01);
			this.gpboption.Controls.Add(this.rdbDetail20);
			this.gpboption.Controls.Add(this.rdbDetail0);
			this.gpboption.Location = new System.Drawing.Point(536, 16);
			this.gpboption.Name = "gpboption";
			this.gpboption.Size = new System.Drawing.Size(496, 56);
			this.gpboption.TabIndex = 5;
			this.gpboption.TabStop = false;
			this.gpboption.Text = "option";
			// 
			// rdbDetail10
			// 
			this.rdbDetail10.Location = new System.Drawing.Point(368, 16);
			this.rdbDetail10.Name = "rdbDetail10";
			this.rdbDetail10.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail10.TabIndex = 8;
			this.rdbDetail10.Text = "10";
			this.rdbDetail10.CheckedChanged += new System.EventHandler(this.rdbDetail10_CheckedChanged);
			// 
			// rdbDetail5
			// 
			this.rdbDetail5.Location = new System.Drawing.Point(328, 16);
			this.rdbDetail5.Name = "rdbDetail5";
			this.rdbDetail5.Size = new System.Drawing.Size(32, 24);
			this.rdbDetail5.TabIndex = 7;
			this.rdbDetail5.Text = "5";
			this.rdbDetail5.CheckedChanged += new System.EventHandler(this.rdbDetail5_CheckedChanged);
			// 
			// rdbDetail2
			// 
			this.rdbDetail2.Location = new System.Drawing.Point(288, 16);
			this.rdbDetail2.Name = "rdbDetail2";
			this.rdbDetail2.Size = new System.Drawing.Size(32, 24);
			this.rdbDetail2.TabIndex = 6;
			this.rdbDetail2.Text = "2";
			this.rdbDetail2.CheckedChanged += new System.EventHandler(this.rdbDetail2_CheckedChanged);
			// 
			// rdbDetail1
			// 
			this.rdbDetail1.Location = new System.Drawing.Point(248, 16);
			this.rdbDetail1.Name = "rdbDetail1";
			this.rdbDetail1.Size = new System.Drawing.Size(32, 24);
			this.rdbDetail1.TabIndex = 5;
			this.rdbDetail1.Text = "1";
			this.rdbDetail1.CheckedChanged += new System.EventHandler(this.rdbDetail1_CheckedChanged);
			// 
			// rdbDetail05
			// 
			this.rdbDetail05.Location = new System.Drawing.Point(200, 16);
			this.rdbDetail05.Name = "rdbDetail05";
			this.rdbDetail05.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail05.TabIndex = 4;
			this.rdbDetail05.Text = "0.5";
			this.rdbDetail05.CheckedChanged += new System.EventHandler(this.rdbDetail05_CheckedChanged);
			// 
			// rdbDetail02
			// 
			this.rdbDetail02.Location = new System.Drawing.Point(152, 16);
			this.rdbDetail02.Name = "rdbDetail02";
			this.rdbDetail02.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail02.TabIndex = 3;
			this.rdbDetail02.Text = "0.2";
			this.rdbDetail02.CheckedChanged += new System.EventHandler(this.rdbDetail02_CheckedChanged);
			// 
			// rdbDetail01
			// 
			this.rdbDetail01.Location = new System.Drawing.Point(104, 16);
			this.rdbDetail01.Name = "rdbDetail01";
			this.rdbDetail01.Size = new System.Drawing.Size(40, 24);
			this.rdbDetail01.TabIndex = 2;
			this.rdbDetail01.Text = "0.1";
			this.rdbDetail01.CheckedChanged += new System.EventHandler(this.rdbDetail01_CheckedChanged);
			// 
			// rdbDetail20
			// 
			this.rdbDetail20.Location = new System.Drawing.Point(408, 16);
			this.rdbDetail20.Name = "rdbDetail20";
			this.rdbDetail20.Size = new System.Drawing.Size(80, 24);
			this.rdbDetail20.TabIndex = 1;
			this.rdbDetail20.Text = "20(coarse)";
			this.rdbDetail20.CheckedChanged += new System.EventHandler(this.rdbDetail20_CheckedChanged);
			// 
			// rdbDetail0
			// 
			this.rdbDetail0.Checked = true;
			this.rdbDetail0.Location = new System.Drawing.Point(8, 16);
			this.rdbDetail0.Name = "rdbDetail0";
			this.rdbDetail0.Size = new System.Drawing.Size(96, 24);
			this.rdbDetail0.TabIndex = 0;
			this.rdbDetail0.TabStop = true;
			this.rdbDetail0.Text = "0 (Everything)";
			this.rdbDetail0.CheckedChanged += new System.EventHandler(this.rdbDetail0_CheckedChanged);
			// 
			// splitter2
			// 
			this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.splitter2.Location = new System.Drawing.Point(0, 597);
			this.splitter2.Name = "splitter2";
			this.splitter2.Size = new System.Drawing.Size(1208, 3);
			this.splitter2.TabIndex = 7;
			this.splitter2.TabStop = false;
			// 
			// dgCallee
			// 
			this.dgCallee.BackgroundColor = System.Drawing.SystemColors.Window;
			this.dgCallee.CaptionBackColor = System.Drawing.SystemColors.ActiveBorder;
			this.dgCallee.DataMember = "";
			this.dgCallee.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.dgCallee.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dgCallee.Location = new System.Drawing.Point(0, 600);
			this.dgCallee.Name = "dgCallee";
			this.dgCallee.Size = new System.Drawing.Size(1208, 260);
			this.dgCallee.TabIndex = 0;
			this.dgCallee.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
																								 this.dataGridTableStyle1});
			this.dgCallee.DoubleClick += new System.EventHandler(this.dgCallee_DoubleClick);
			this.dgCallee.MouseMove += new System.Windows.Forms.MouseEventHandler(this.dgCallee_MouseMove);
			// 
			// dataGridTableStyle1
			// 
			this.dataGridTableStyle1.DataGrid = this.dgCallee;
			this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.dataGridTableStyle1.MappingName = "";
			this.dataGridTableStyle1.PreferredColumnWidth = 200;
			// 
			// panel2
			// 
			this.panel2.Controls.Add(this.tabControl1);
			this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
			this.panel2.Location = new System.Drawing.Point(0, 0);
			this.panel2.Name = "panel2";
			this.panel2.Size = new System.Drawing.Size(1216, 886);
			this.panel2.TabIndex = 2;
			// 
			// ReportForm
			// 
			this.ClientSize = new System.Drawing.Size(1216, 886);
			this.Controls.Add(this.panel2);
			this.Name = "ReportForm";
			this.Text = "Comparing Allocations and Calls";
			this.tabControl1.ResumeLayout(false);
			this.tabpallocdiff.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgToplevelDiff)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgCaller)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.dgSelected)).EndInit();
			this.panel1.ResumeLayout(false);
			this.gpboption.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.dgCallee)).EndInit();
			this.panel2.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion
	}
}

