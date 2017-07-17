// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Reflection;
using System.Data;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for AllocationDiffForm.
	/// Table usage:
	/// All Tables created by AllocationDiff object been used by ReportForms 
	///		basedatatable - appears in Function Allocation Diff datagrid
	///		caller - appears in Caller datagrid
	///		callee - appears in Callee datagrid (if selected Item was function)
	///		typeAlloction - appears in Callee datagrid also (if selected Item was data type)
	/// UI interface:
	///		DataGrid object binding above internal tables
	///		DataGridTableStyle controls the appearance of the columns for each table
	///		DataViewManager used for customized view of the entire DataSet
	///		DataViewSettings used to set filters and sort options for a given table. 
	///		DataGrid alignment been controlled by datagrid column width
	///		table content order based on diff Inclusive size (DESC order)
	///		
	/// Detail Definitions:
	///		Memory allocation report can show 9 different details by choose 9 different RadioButton
	///		detail0 shows everything and detail20 only shows diff 
	///	
	///		
	/// </summary>
	public partial class ReportForm : System.Windows.Forms.Form
	{
		private const int idx_id = 2;
		private const int idx_name = 3;
		private const int idx_mapname = 4;
		private const int idx_prevIncl = 5;
		private const int idx_currIncl = 6;
		private const int idx_diffIncl = 7;
		private const int idx_depth = 14;

	    [NotNull] private readonly DataGridTableStyle styleBase = new DataGridTableStyle();
	    [NotNull] private readonly DataGridTableStyle styleCaller = new DataGridTableStyle();
	    [NotNull] private readonly DataGridTableStyle styleCallee = new DataGridTableStyle();
	    [NotNull] private readonly DataGridTableStyle styleSelected = new DataGridTableStyle();

        private DiffCallTreeForm diffcallTreeForm;
		private AllocationDiff	_allocDiff;
		private bool iscoarse = false;
		private string strFilter;
		private string strtypeFilter;
		private readonly string CallerCaption = "Caller table";
		private readonly string SelectedCaption = "Selected item";
		private readonly string CalleeCaption = "Callee table";
		private readonly Graph.GraphType graphtype;
	    [NotNull] private readonly MainForm f;
		private readonly string [] columName = {"name", 
								"prevIncl", "currIncl", "diffIncl", 
								"prevExcl", "currExcl", "diffExcl", 
								"prevChildIncl", "currChildIncl", "diffChildIncl",
								"prevTimesCalled", "currTimesCalled", "diffTimesCalled",
								"prevTimesMakecalls", "currTimesMakecalls", "diffTimesMakecalls"};
		private enum columnIdx {name, prevIncl, currIncl, diffIncl,
										prevExcl,currExcl,diffExcl, 
										prevChildIncl,currChildIncl,diffChildIncl,
										prevTimesCalled,currTimesCalled,diffTimesCalled,
										prevTimesMakecalls,currTimesMakecalls,diffTimesMakecalls}

		private readonly string [] coarsecolumName = {"name", 
										  "diffIncl", 
										  "diffExcl", 
										  "diffTimesCalled",
										  "diffTimesMakecalls"};
	
		private enum coarsecolumnIdx {name, diffIncl,diffExcl,diffTimesCalled,diffTimesMakecalls}

		#region Report -- Entrance
		internal ReportForm([NotNull] MainForm f)
		{
			this.f = f;
			this.graphtype = f.graphtype;
			if (!f.noUI)
			{
				InitializeComponent();
			}
			try
			{
				buildReports();
			}
			catch(Exception e)
			{
				throw new Exception(e.Message + "\n");
			}
		}
		private void buildReports()
		{
			try
			{
				switch(graphtype)
				{
					case Graph.GraphType.AllocationGraph:
						_allocDiff = new AllocationDiff ();
						_allocDiff.PrevLogFileName = f.prevlogFileName;
						_allocDiff.CurrLogFileName = f.currlogFileName;
						_allocDiff.diffLogFileName = f.difflogFileName;
						_allocDiff.BuildAllocationDiffTable();
						if (!f.noUI)
						{
							this.txtbPrevlog.Text = f.prevlogFileName;
							this.txtbCurrlog.Text = f.currlogFileName;
							AlloccationDiff2Window();
						}
						else
						{
							AlloccationDiff2Console();
						}					
						break;
				

				}
			}
			catch(Exception e)
			{
				if (!f.noUI)
				{
					MessageBox.Show(e.Message, "Report profiler Error message");
					throw new Exception(e.Message + "\n");
				}
				else
				{
					if(_allocDiff.diffLogFileName != null)
					{
						int at = _allocDiff.diffLogFileName.LastIndexOf(".");
						_allocDiff.diffLogFileName = _allocDiff.diffLogFileName.Substring(0,at);
						_allocDiff.diffLogFileName += ".err";
						if(File.Exists(_allocDiff.diffLogFileName))
                        {
                            File.Delete(_allocDiff.diffLogFileName);
                        }

                        var fs = new FileStream(_allocDiff.diffLogFileName,
							FileMode.CreateNew, FileAccess.Write, FileShare.None);
						var sw = new StreamWriter(fs);
						sw.WriteLine("Report profiler Error message: \n{0}\n",  e.Message);
						sw.Close();
						
					}
					else
					{
						Console.WriteLine("Report profiler Error message: \n{0}\n",  e.Message);
					}
					throw new Exception(e.Message + "\n");
				}
			}
		}

		#endregion

		#region Clean up
		#endregion

		
		#region Diff -- Runas console
		private void AlloccationDiff2Console()
		{
			try
			{
				DataRow[] topdiff = _allocDiff.basedatatable.Select("diffIncl <> 0", "diffIncl DESC");
				if(_allocDiff.diffLogFileName != null)
				{
					if( _allocDiff.diffLogFileName != _allocDiff.PrevLogFileName &&  _allocDiff.diffLogFileName != _allocDiff.CurrLogFileName)
					{
						var sb = new StringBuilder();
						sb.AppendFormat("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  "Function/type Name", "Inclusive", "Diff_Inclusive(KB)", "timesBeenCalled", "Diff_timesBeenCalled", "timesCalls", "Diff_timesCalls");
						for(int i = 0; i< topdiff.Length; i++)
						{
							sb.AppendFormat("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  topdiff[i][1], topdiff[i][3],topdiff[i][4], topdiff[i][12],topdiff[i][13],topdiff[i][15],topdiff[i][16]);
						}
						
						if(File.Exists(_allocDiff.diffLogFileName))
                        {
                            File.Delete(_allocDiff.diffLogFileName);
                        }

                        var fs = new FileStream(_allocDiff.diffLogFileName,
							FileMode.CreateNew, FileAccess.Write, FileShare.None);
						var sw = new StreamWriter(fs);
						sw.Write(sb);
						sw.Close();
					}
					else
					{
						if (!f.noUI)
						{
							MessageBox.Show("log file name are same!","Report profiler Error message");
						}
						else
						{
							Console.WriteLine("Report profiler Error message: \n{0}\n",  "log file name are same!");
						}
					}
				}
				else
				{
					Console.WriteLine("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  "Function/type Name", "Inclusive", "Diff_Inclusive(KB)", "timesBeenCalled", "Diff_timesBeenCalled", "timesCalls", "Diff_timesCalls");
					for(int i = 0; i< topdiff.Length; i++)
					{
						Console.WriteLine("{0}\t {1}\t {2}\t {3}\t {4}\t {4}\t {6}\r\n",  topdiff[i][1], topdiff[i][3],topdiff[i][4], topdiff[i][12],topdiff[i][13],topdiff[i][15],topdiff[i][16]);
					}
				}
			}
			catch(Exception e)
			{
				if (!f.noUI)
				{
					MessageBox.Show(e.Message, "Report profiler Error message");
				}
				else
				{
					Console.WriteLine("Report profiler Error message: \n{0}\n",  e.Message);
				}
			}

		}
		#endregion

		#region Diff -- Runas window
		private void AlloccationDiff2Window()
		{
			clearTableStyle();
			createTableStyle();
			strFilter = "prevIncl >= 0"; 
			strtypeFilter = "prevExcl >= 0"; 
			rdbDetail0.Checked = true;
			InitDataGridBinding();
			diffcallTreeForm = new DiffCallTreeForm(_allocDiff.Root, _allocDiff);
			diffcallTreeForm.Visible = true;
			
		}
		#endregion
	
		#region Table Styles	
		// DataGridTableStyle controls the appearance of the columns for each table 
		private void clearTableStyle()
		{
			if(dgToplevelDiff.TableStyles.Contains(this.styleBase))
			{
				styleBase.GridColumnStyles.Clear();
				dgToplevelDiff.TableStyles.Remove(this.styleBase);
			}
			if(dgCallee.TableStyles.Contains(this.styleCallee))
			{
				this.styleCallee.GridColumnStyles.Clear();
				dgCallee.TableStyles.Remove(this.styleCallee);
			}
			
			if(dgCaller.TableStyles.Contains(this.styleCaller))
			{
				this.styleCaller.GridColumnStyles.Clear();
				dgCaller.TableStyles.Remove(this.styleCaller);
			}

			if(dgSelected.TableStyles.Contains(this.styleSelected))
			{
				this.styleSelected.GridColumnStyles.Clear();
				dgSelected.TableStyles.Remove(this.styleSelected);
			}
		
		}

		private void createTableStyle()
		{
			Detail0Style(dgToplevelDiff,_allocDiff.basedatatable, styleBase, 2);
			Detail0Style(dgCaller, _allocDiff.ContriTocallertbl, styleCaller, 7);
			Detail0Style(dgSelected, _allocDiff.basedatatable,styleSelected, 10);
			Detail0Style(dgCallee, _allocDiff.ContriTocalleetbl, styleCallee, 7);
		}
		private void Detail0Style([NotNull] DataGrid dg, [NotNull] DataTable dt, [NotNull] DataGridTableStyle style, int dghightf)
		{
			if(!dg.TableStyles.Contains(style))
			{
				int with = this.Size.Width;
				dg.Height = this.Height * 1/dghightf;
				
				style.MappingName = dt.TableName;
				style.AlternatingBackColor = System.Drawing.Color.Beige;
				var name = new  DataGridTextBoxColumn();
				name.HeaderText = "Function names";
				name.MappingName = "name";
				name.Width = with * 55/100;

				var prev = new DataGridTextBoxColumn();
				prev.HeaderText = "Old Incl (KB)";
				prev.MappingName = "prevIncl";
			
				var curr = new DataGridTextBoxColumn();
				curr.HeaderText = " New Incl (KB)";
				curr.MappingName = "currIncl";
			
				var diff = new DataGridTextBoxColumn();
				diff.HeaderText = "Diff Incl (KB)";
				diff.MappingName = "diffIncl";
	
				var prevExcl = new DataGridTextBoxColumn();
				prevExcl.HeaderText = "Old Excl (KB)";
				prevExcl.MappingName = "prevExcl";
			
				var currExcl= new DataGridTextBoxColumn();
				currExcl.HeaderText = "New Excl (KB)";
				currExcl.MappingName = "currExcl";
			
				var diffExcl = new DataGridTextBoxColumn();
				diffExcl.HeaderText = "Diff Excl (KB)";
				diffExcl.MappingName = "diffExcl";

				var prevChildIncl = new DataGridTextBoxColumn();
				prevChildIncl.HeaderText = "Old ChildIncl (KB)";
				prevChildIncl.MappingName = "prevChildIncl";

				var currChildIncl = new DataGridTextBoxColumn();
				currChildIncl.HeaderText = "New ChildIncl (KB)";
				currChildIncl.MappingName = "currChildIncl";

				var diffChildIncl = new DataGridTextBoxColumn();
				diffChildIncl.HeaderText = "Diff ChildIncl (KB)";
				diffChildIncl.MappingName = "diffChildIncl";

				var prevTimesCalled = new DataGridTextBoxColumn();
				prevTimesCalled.HeaderText = "# Old Called";
				prevTimesCalled.MappingName = "prevTimesCalled";

				var currTimesCalled = new DataGridTextBoxColumn();
				currTimesCalled.HeaderText = "# New  Called";
				currTimesCalled.MappingName = "currTimesCalled";

				var diffTimesCalled = new DataGridTextBoxColumn();
				diffTimesCalled.HeaderText = "# Diff Called";
				diffTimesCalled.MappingName = "diffTimesCalled";

				var prevTimesMakecalls = new DataGridTextBoxColumn();
				prevTimesMakecalls.HeaderText = "# Old Calls";
				prevTimesMakecalls.MappingName = "prevTimesMakecalls";

				var currTimesMakecalls = new DataGridTextBoxColumn();
				currTimesMakecalls.HeaderText = "# New Calls";
				currTimesMakecalls.MappingName = "currTimesMakecalls";

				var diffTimesMakecalls = new DataGridTextBoxColumn();
				diffTimesMakecalls.HeaderText = "# Diff Calls";
				diffTimesMakecalls.MappingName = "diffTimesMakecalls";

				
				if( !rdbDetail20.Checked)
				{

					style.GridColumnStyles.AddRange(new DataGridColumnStyle[]{name,
																				 prev, curr, diff,
																				 prevExcl, currExcl, diffExcl,
																				 prevChildIncl, currChildIncl, diffChildIncl,
																				 prevTimesCalled, currTimesCalled, diffTimesCalled,
																				 prevTimesMakecalls, currTimesMakecalls, diffTimesMakecalls});
				}
				else
				{
					style.GridColumnStyles.AddRange(new DataGridColumnStyle[]{name,diff, diffExcl,diffTimesCalled,diffTimesMakecalls});
				}
				dg.TableStyles.Add(style);
			}
			
		}
		#endregion

		#region Table filter
		private void rdbDetail0_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail0.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail20_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail20.Checked)
			{
				iscoarse = true;
				ResetTableStyle();
			}
			TableLevelfilter();
		}
		
	
		private void rdbDetail01_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail01.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail02_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail02.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();

		}

		private void rdbDetail05_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail05.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail1_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail1.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail2_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail2.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail5_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail5.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}

		private void rdbDetail10_CheckedChanged(object sender, System.EventArgs e)
		{
			if( rdbDetail10.Checked && iscoarse)
			{
				iscoarse = false;
				ResetTableStyle();
			}
			TableLevelfilter();
		}
		private void ResetTableStyle()
		{
			clearTableStyle();
			createTableStyle();
			dgToplevelDiff.Visible = true;
			dgCaller.Visible = true;
			dgCallee.Visible = true;
			dgSelected.Visible = true;
		}

		private void TableLevelfilter()
		{
			if( rdbDetail0.Checked)
			{
				strFilter = "prevIncl >= 0"; 
				strtypeFilter = "prevExcl >= 0"; 
			}
			else if(rdbDetail01.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail01 + ") or (currIncl > " + _allocDiff.currFilter.detail01 + ")"; 
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail01 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail01 + ")"; 
				//strtypeFilter = "(prevExcl > (max(prevExcl) / 8)) or (currExcl > (max(currExcl) / 8))"; 
			}
			else if(rdbDetail02.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail02 + ") or (currIncl > " + _allocDiff.currFilter.detail02 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail02 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail02 + ")";
			}
			else if(rdbDetail05.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail05 + ") or (currIncl > " + _allocDiff.currFilter.detail05 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail05 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail05 + ")";
			}
			else if(rdbDetail1.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail1 + ") or (currIncl > " + _allocDiff.currFilter.detail1 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail1 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail1 + ")";
			}
			else if(rdbDetail2.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail2 + ") or (currIncl > " + _allocDiff.currFilter.detail2 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail2 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail2 + ")";
			}
			else if(rdbDetail5.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail5 + ") or (currIncl > " + _allocDiff.currFilter.detail5 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail5 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail5 + ")";
			}
			else if(rdbDetail10.Checked)
			{
				strFilter = "(prevIncl > " + _allocDiff.prevFilter.detail10 + ") or (currIncl > " + _allocDiff.currFilter.detail10 + ")";
				strtypeFilter = "(prevExcl > " + _allocDiff.prevTypedeFilter.detail10 + ") or (currExcl > " + _allocDiff.currTypedeFilter.detail10 + ")";
			}
			InitDataGridBinding();			
		}
		#endregion

		#region View -- DataGrids
		private void dgToplevelDiff_DoubleClick(object sender, System.EventArgs e)
		{
			int roeNumber = dgToplevelDiff.CurrentCell.RowNumber;
			string name = (string)dgToplevelDiff[roeNumber, 0];
			int prevIncl = (int)dgToplevelDiff[roeNumber, 1];
			int currIncl = (int)dgToplevelDiff[roeNumber, 2];
			int diffIncl = (int)dgToplevelDiff[roeNumber, 3];
			
			
			ViewSelectedInfo(dgToplevelDiff);
			ViewCallTrace(name, prevIncl, currIncl, diffIncl);
		}
		private void dgCaller_DoubleClick(object sender, System.EventArgs e)
		{
			ViewSelectedInfo(dgCaller);
		}

		private void dgCallee_DoubleClick(object sender, System.EventArgs e)
		{
			int roeNumber = dgCallee.CurrentCell.RowNumber;
			string name = (string)dgCallee[roeNumber, 0];
			int prevIncl = (int)dgCallee[roeNumber, 1];
			int currIncl = (int)dgCallee[roeNumber, 2];
			int diffIncl = (int)dgCallee[roeNumber, 3];
			
			ViewSelectedInfo(dgCallee);
			ViewCallTrace(name, prevIncl, currIncl, diffIncl);
		}

		private void ViewSelectedInfo(DataGrid dg)
		{
			if( dg.DataMember == "")
            {
                return;
            }

            dvm = new DataViewManager(_allocDiff.ds);
			dvm_caller = new DataViewManager(_allocDiff.ds);
			dvm_callee = new DataViewManager(_allocDiff.ds);
			
			int roeNumber = dg.CurrentCell.RowNumber;
            		
			string name = (string)dg[roeNumber, 0];
			int id = (int)_allocDiff.basedataId[name];
			DataRow[] caller = _allocDiff.callertbl.Select("id =" + id);
			DataRow[] callee = _allocDiff.calleetbl.Select("id =" + id);
			
			//======== caller table============
			if(name != "<root>")
			{
				dgCaller.CaptionText = CallerCaption;
			    // FIXME
			    // ReSharper disable once PossibleNullReferenceException
                dvm_caller.DataViewSettings[_allocDiff.ContriTocallertbl.TableName].RowFilter = "id = " + id;
				dgCaller.SetDataBinding(dvm_caller ,_allocDiff.ContriTocallertbl.TableName);
			}
			else
			{
				dgCaller.SetDataBinding(null, null);
			}

			dgCaller.Visible = true;

			//========= selected table ==========
			dgSelected.CaptionText = SelectedCaption;
		    // FIXME
            // ReSharper disable once PossibleNullReferenceException
			dvm.DataViewSettings[_allocDiff.basedatatable.TableName].RowFilter = "id=" + id;
			dgSelected.SetDataBinding(dvm,_allocDiff.basedatatable.TableName);
			dgSelected.Visible = true;

			//========= callee Table ===============
			if(name != "<bottom>")
			{
				dgCallee.CaptionText = CalleeCaption;
			    // FIXME
			    // ReSharper disable once PossibleNullReferenceException
                dvm_callee.DataViewSettings[_allocDiff.ContriTocalleetbl.TableName].RowFilter = "id = " + id;
				dgCallee.SetDataBinding(dvm_callee ,_allocDiff.ContriTocalleetbl.TableName);
			}
			else
			{
				dgCallee.SetDataBinding(null, null);
			}
			dgCallee.Visible = true;
			this.Invalidate();
				
				
				
			
		}
		#endregion

		#region View -- ViewCallTrace
		private void ViewCallTrace(string name, int prevIncl, int currIncl, int diffIncl)
		{
			if(name == "<root>")
			{
				diffcallTreeForm = new DiffCallTreeForm(_allocDiff.Root, _allocDiff);
				return;
			}
						
			var root = new DiffDataNode(name);
			root.mapname = name;
			root.prevIncl = prevIncl;
			root.currIncl = currIncl;
			root.diffIncl = diffIncl;
			root.currFunId = -1;
			root.prevFunId = -1;

			var tmproot = new DiffDataNode(name);
			tmproot.mapname = name;
		    string filter = null;
			DataRow[] rRoot = null;
			
			if(_allocDiff._currcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash.ContainsKey(name))
			{
				root.currFunId = _allocDiff._currcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash[name];
			}
			if(_allocDiff._prevcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash.ContainsKey(name))
			{
				root.prevFunId = _allocDiff._prevcallTrace.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash[name];
			}
			if(!(root.prevFunId == -1 && root.currFunId == -1))
			{
				filter = "(prevIncl = " + prevIncl + ") and (currIncl = " + currIncl + ") and (prevFunId = " + root.prevFunId + ") and (currFunId = " + root.currFunId + ")";
				rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
				
				if(rRoot.Length ==0 )
				{
					filter = "(prevFunId = " + root.prevFunId + ") or (currFunId = " + root.currFunId + ")";
					rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
					
					if(rRoot.Length == 0)
					{
						filter = "(currFunId = " + root.currFunId + ") and (prevFunId = -1)";
						rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
						
						if(rRoot.Length == 0)
						{
							filter = "(prevFunId = " + root.prevFunId + ") and (currFunId = -1)";
							rRoot = _allocDiff.summaryTracetbl.Select(filter, "depth asc");
							if(rRoot.Length == 0)
							{
								filter = null;
							}
						}
					}
				}
			}
		
			if(filter != null)
			{
			    string diffkey;
			    if(rRoot.Length == 1)
				{
					tmproot = _allocDiff.Row2Node(rRoot[0]);
					diffkey = tmproot.mapname + tmproot.prevIncl + tmproot.currIncl + tmproot.diffIncl + tmproot.prevFunId + tmproot.currFunId;
					root = _allocDiff.diffCallTreeNodes[diffkey] as DiffDataNode;
				}
				else if(rRoot.Length > 1 )
				{
					long sum = 0;
					string depth = null;
					long diffincl = 0;
					var depLst = new Hashtable();
					var depSum = new Hashtable();
					for(int i = 0; i < rRoot.Length; i++)
					{
						depth = rRoot[i][idx_depth].ToString();
						diffincl= long.Parse(rRoot[i][idx_diffIncl].ToString());
						sum += diffincl;
						if(depLst.Contains(depth))
						{
							var alst = (ArrayList)depLst[depth];
							alst.Add(rRoot[i]);
							depLst[depth] = alst;
							diffincl += (long)depSum[depth];
						}
						else
						{
							var alst = new ArrayList();
							alst.Add(rRoot[i]);							
							depLst.Add(depth, alst);
							depSum.Add(depth, diffincl);
						}
						if(diffincl == root.diffIncl)
                        {
                            break;
                        }
                    }

				    Debug.Assert(root.allkids != null);
					if(sum != root.diffIncl && diffincl == root.diffIncl )
					{
                        Debug.Assert(depth != null);
						if(depLst.ContainsKey(depth))
						{
							var lst = (ArrayList)depLst[depth];
							for(int i = 0; i < lst.Count; i++)
							{
								var r = (DataRow)lst[i];
								tmproot = _allocDiff.Row2Node(r);
								diffkey = tmproot.mapname + tmproot.prevIncl + tmproot.currIncl + tmproot.diffIncl + tmproot.prevFunId + tmproot.currFunId;
								var subRoot =  (DiffDataNode) _allocDiff.diffCallTreeNodes[diffkey];
								root.allkids.Add(subRoot);
							}
						}
					}
					else
					{

						for(int i = 0; i < rRoot.Length; i++)
						{
							tmproot = _allocDiff.Row2Node(rRoot[i]);
							if(tmproot.depth > 0)
							{
								diffkey = tmproot.mapname + tmproot.prevIncl + tmproot.currIncl + tmproot.diffIncl + tmproot.prevFunId + tmproot.currFunId;
								var subRoot =  (DiffDataNode) _allocDiff.diffCallTreeNodes[diffkey];
								root.allkids.Add(subRoot);
							}
						}
					}
					root.HasKids = true;
					root.depth = 0;

				}
				diffcallTreeForm.Close();
				diffcallTreeForm = new DiffCallTreeForm(root, _allocDiff);
			}
			
		}
		
	
		#endregion

		#region Aligement -- DataGrid columns 
		// DataGrid alignment been controlled by datagrid column width
        //FIXME what causes this?
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		private void InitDataGridBinding()
		{
			dvm = new DataViewManager(_allocDiff.ds);
			dvm.DataViewSettings[_allocDiff.basedatatable.TableName].RowFilter = strFilter;
			dvm.DataViewSettings[_allocDiff.basedatatable.TableName].Sort = "diffIncl desc";
			
			dgToplevelDiff.CaptionText = "Function allocation diff";
			dgToplevelDiff.SetDataBinding(dvm, _allocDiff.basedatatable.TableName);
			dgCallee.SetDataBinding(null, null);
			dgCaller.SetDataBinding(null, null);
			dgSelected.SetDataBinding(null, null);
		}
		
		private void dgToplevelDiff_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleBase);
		}

		private void dgCaller_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleCaller);
		}

		private void dgSelected_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleSelected);
		}

		private void dgCallee_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			aligDataGrids(sender, e, styleCallee);
		}

		private void aligDataGrids(object sender, System.Windows.Forms.MouseEventArgs e, DataGridTableStyle sty)
		{
		    DataGrid.HitTestInfo hi = ((DataGrid) sender).HitTest(e.X, e.Y);
		    if(!iscoarse)
			{
				columnIdx idx = (columnIdx)hi.Column-1;
				if(idx >= columnIdx.name && idx <= columnIdx.diffTimesMakecalls)
				{
					aligEverythingDataGrid(sty, idx);
				}
			}
			else
			{
				coarsecolumnIdx idx = (coarsecolumnIdx)hi.Column-1;
				if(idx >= coarsecolumnIdx.name && idx <= coarsecolumnIdx.diffTimesMakecalls)
				{
					aligcoarseDataGrid(sty, idx);
				}
				
			}
		}

	    //FIXME what causes this?
        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		private void aligEverythingDataGrid([NotNull] DataGridTableStyle sty, columnIdx idx)
		{
			string colName = columName[(int)idx];
			styleBase.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCaller.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCallee.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleSelected.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
		}

        //FIXME what causes this?
		[SuppressMessage("ReSharper", "PossibleNullReferenceException")]
		private void aligcoarseDataGrid([NotNull] DataGridTableStyle sty, coarsecolumnIdx idx)
		{
			string colName = coarsecolumName[(int)idx];
			styleBase.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCaller.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleCallee.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
			styleSelected.GridColumnStyles[colName].Width = sty.GridColumnStyles[colName].Width;
		}
		#endregion

		#region Event -- Button

		private void btnBrowse1_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = "*.log";
			openFileDialog1.Filter = "Allocation Logs | *.log";
			if (   openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.CheckFileExists)
			{
				f.prevlogFileName = openFileDialog1.FileName;
				this.txtbPrevlog.Text = f.prevlogFileName;
			}
		}

		private void btnBrowse2_Click(object sender, System.EventArgs e)
		{
			openFileDialog1.FileName = "*.log";
			openFileDialog1.Filter = "Allocation Logs | *.log";
			if (   openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.CheckFileExists)
			{
				f.currlogFileName = openFileDialog1.FileName;
				this.txtbCurrlog.Text = f.currlogFileName;
			}
		
		}

		private void btnRun_Click(object sender, System.EventArgs e)
		{
			string prevfile = this.txtbPrevlog.Text;
			string currfile = this.txtbCurrlog.Text;
			if(File.Exists(prevfile) && File.Exists(currfile))
			{
				f.currlogFileName = currfile;
				f.prevlogFileName = prevfile;
				buildReports();			
			}
		}

		#endregion


		
		
	}
	
}
