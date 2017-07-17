//#define V_EXEC	

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Data;
using System.Diagnostics;
using JetBrains.Annotations;
#if V_EXEC
using DoubleInt = double;
using DoubleUInt64 = double;
#else
using DoubleInt = System.Int32;
using DoubleUInt64 = System.UInt64;
#endif



namespace CLRProfiler
{
    /// <summary>
    /// Code organize:
    ///		AllocationDiff reads two log files one from previous build and one from current build. 
    ///			see region - get base data methods
    ///		also it builds three tables,
    ///			see region - build Base table
    ///			see region - build Caller and callee table
    ///			see region - build Type Allocation table
    ///			
    ///	internal data structure:
    ///		prevLog and currLog object hold base log data from log files
    ///		prevG and currG object hold base Graph data, the main purpose was keep the call relations
    ///		4 hash table (prevbasedata, currbasedata, prevtypeAllocdata, currtypeAllocdata) holds all useful data from base log/graph
    ///		been used for build all diff, call relations, and type allocation tables. 
    ///		
    ///	Table Definitions:
    ///		basedatatable contains all basic data: inclusive, exclusive, diff, childinclusive, childexclusive, timesBeenCalled, timesmakescalls.
    ///		caller table contains function Id and its caller Ids 
    ///		callee table contains function Id and its callee Ids
    ///		typeAlloction table contains type exclusive and diff info
    ///	
    ///	Detail Definitions:
    ///		Memory allocation report can show 9 different details based on allocated memory size
    ///		detail0 - all size
    ///		detail01 = max(prevIncl) / 8
    ///		detail02 = max(prevIncl) /7
    ///		...
    ///		
    /// </summary>

    public class AllocationDiff 
	{
		class CompareIncl : IComparer
		{
		    [NotNull] readonly Hashtable inclOfNode;

			internal CompareIncl([NotNull] Hashtable inclOfNode)
			{
				this.inclOfNode = inclOfNode;
			}

			int IComparer.Compare(object x, object y)
			{
				long inclX = (long)inclOfNode[x];
				long inclY = (long)inclOfNode[y];
				if (inclX < inclY)
                {
                    return 1;
                }
                else if (inclX > inclY)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
		}

		#region data member
		// log file names
		private string _prevFile;
		private string _currFile;

	    // holds all useful data from base
		// for build all diff, call relations, and type allocation tables
	    [NotNull] internal readonly Hashtable _prevbasedata = new Hashtable();

	    [NotNull] internal readonly Hashtable _currbasedata = new Hashtable();


		// maps for match search
	    [NotNull] public readonly Hashtable basedataId = new Hashtable();

	    [NotNull] public readonly Hashtable Idbasedata = new Hashtable();
		public Hashtable typeAllocdataId = new Hashtable();

		// hold log data		
	    [NotNull] private readonly LogBase _prevLog = new LogBase();

	    [NotNull] private readonly LogBase _currLog = new LogBase();
		// hold base graph related data
	    [NotNull] private readonly GraphBase _prevG = new GraphBase();

	    [NotNull] private readonly GraphBase _currG = new GraphBase();

		// hold base stacktrace info
		internal CallTreeForm _prevcallTrace;
		internal CallTreeForm _currcallTrace;
		internal DiffDataNode Root;
		internal DataTable diffTracetbl;
		private static int nodeidx = 0;

	    [NotNull] internal readonly Hashtable prevFuncExcl = new Hashtable();
	    [NotNull] internal readonly Hashtable currFuncExcl = new Hashtable();
	    [NotNull] internal readonly Hashtable prevTypeExcl = new Hashtable();
	    [NotNull] internal readonly Hashtable currTypeExcl = new Hashtable();


		StreamReader r;
		byte[] buffer;
		int c;
		int line;
		long pos;
		long lastLineStartPos;
		int bufPos;
		int bufLevel;
		private static int sumnodeidx = 0;
		private static int depth = 0;

		internal DataTable summaryTracetbl;
	    [NotNull] internal readonly Hashtable diffCallTreeNodes = new Hashtable();
				
		private const int idx_parentid = 0;
	//	private const int idx_parentname = 1;
		private const int idx_id = 2;
		private const int idx_name = 3;
		private const int idx_mapname = 4;
		private const int idx_prevIncl = 5;
		private const int idx_currIncl = 6;
		private const int idx_diffIncl = 7;
		
		private const int idx_prevCalls = 8;
		private const int idx_currCalls = 9;
		private const int idx_diffCalls = 10;
		
		private const int idx_prevFunid = 11;
		private const int idx_currFunid = 12;
		private const int idx_type = 13;
		private const int idx_depth = 14;
		

		// table details filter value
#if (V_EXEC)
		private const double detail01D = 8.0;
#else
		private const int detail01D = 8;
#endif
		private ulong maxIncl = 0;
		private ulong typemaxIncl = 0;
		public DetailFilter prevFilter;
		public DetailFilter currFilter;
		public DetailFilter prevTypedeFilter;
		public DetailFilter currTypedeFilter;
		
		// dataset and tables

	    //private DataTable _typeAlloctable = null;
	    #endregion
		
		#region struct data methods
		
		// caller and callee tables node
		struct callnode
		{
		    public int id { get; set; }

		    public int callerid { get; set; }

		    public int calleeid { get; set; }
		}
		// typeAllocation table node
		struct typeAllocnode
		{
			public int typeid { get; set; }

		    public int funcid { get; set; }
            
            public DoubleInt allocmem { get; set; }
			
		}

	    [NotNull]
	    public DataTable basedatatable { get; } = new DataTable("basedatatbl");

	    [NotNull]
	    public DataTable ContriTocallertbl { get; } = new DataTable("ContriTocallertbl");

	    [NotNull]
	    public DataTable ContriTocalleetbl { get; } = new DataTable("ContriTocalleetbl");

	    // detailds for reportform details RadioButton
		public struct DetailFilter
		{
            internal DoubleUInt64 detail01 { get; set; }
		    internal DoubleUInt64 detail02 { get; set; }
		    internal DoubleUInt64 detail05 { get; set; }
            internal DoubleUInt64 detail1 { get; set; }
            internal DoubleUInt64 detail2 { get; set; }
            internal DoubleUInt64 detail5 { get; set; }
            internal DoubleUInt64 detail10 { get; set; }
            internal ulong max { get; set; }
        }
		
		#endregion

		#region constructor
		public AllocationDiff()
	    {
	        MakeBaseDataTable(basedatatable);
            MakeCallerTables(callertbl);
            MakeCalleeTables(calleetbl);
	        MakeBaseDataTable(ContriTocallertbl);
	        MakeBaseDataTable(ContriTocalleetbl);
	    }
	    #endregion

		#region public property methods
		// DataSet used to collect tables and 
		// build relations between table in the near future
		// also it usded by DataViewManager in ReportForm
	    [NotNull]
	    public DataSet ds { get; } = new DataSet();

	    [NotNull]
	    public DataTable callertbl { get; } = new DataTable("caller");

	    [NotNull]
	    public DataTable calleetbl { get; } = new DataTable("callee");

	    public string PrevLogFileName
		{
			get => _prevFile;
	        set
			{
				_prevFile = value;
				_prevLog.LogFileName = value;
            }
		}

		public string CurrLogFileName
		{
			get => _currFile;
		    set
			{
				_currFile = value;
				_currLog.LogFileName = value;
			}
		}

	    [CanBeNull]
	    public string diffLogFileName { get; set; }
	    #endregion

		#region public methods
		public void BuildAllocationDiffTable()
		{
			try
			{
				GetLogData();
				//BuildBaseData(_prevG, _prevcallTree, _prevbasedata);
				//BuildBaseData(_currG, _currcallTree, _currbasedata);
				
				BuildBaseData(_prevG, _prevcallTrace, _prevbasedata, this.prevFuncExcl, this.prevTypeExcl);
				BuildBaseData(_currG, _currcallTrace, _currbasedata, this.currFuncExcl, this.currTypeExcl);
				BuildBaseDataTable();
				BuildBaseCallTables();
				getDetailFilter(ref prevFilter);
				getDetailFilter(ref currFilter);
				getDetailFilter(ref prevTypedeFilter);
				getDetailFilter(ref currTypedeFilter);
								
				BuildContributionCalleeTable();
				BuildContributionCallerTable();
			}
			catch(Exception e)
			{
				throw new Exception(e.Message + "\n");
			}
		}
		public bool IsAllocType(string name)
		{
			return (_prevLog.logResult.allocatedHistogram.readNewLog.typeSignatureIdHash.ContainsKey(name) ||
				_currLog.logResult.allocatedHistogram.readNewLog.typeSignatureIdHash.ContainsKey(name));
		}
		#endregion

		#region GetLogData
		private void GetLogData()
		{
			// get base data from log files
			try
			{
				_prevLog.readLogFile();
			}
			catch
			{
				throw new Exception("Bad log file: " + _prevLog.LogFileName + "\n");
			}
			try
			{
				_currLog.readLogFile();
			}
			catch
			{
				throw new Exception("Bad log file: " + _currLog.LogFileName +  "\n");
			}

			// get mixed data structure graph
			try
			{
				_prevG.GetAllocationGraph(_prevLog.logResult);
			}
			catch
			{
				throw new Exception("Bad data structure in log file: " + _prevLog.LogFileName +  "\n");
			}
			try
			{
				_currG.GetAllocationGraph(_currLog.logResult);
			}
			catch
			{
				throw new Exception("Bad data structure in log file: " + _currLog.LogFileName +  "\n");
			}

			// get more detailed allocation info from stack trace 
			try
			{
				
				_prevcallTrace = new CallTreeForm(_prevLog.LogFileName, _prevLog.logResult, true);
				ReadFile(_prevcallTrace, _prevLog.LogFileName, this.prevFuncExcl, this.prevTypeExcl);
			}
			catch
			{
				throw new Exception("Bad stacktrace content in log file: " + _prevLog.logResult +  "\n");
			}
			try
			{
				_currcallTrace = new CallTreeForm(_currLog.LogFileName, _currLog.logResult, true);
				ReadFile(_currcallTrace, _currLog.LogFileName, this.currFuncExcl, this.currTypeExcl);
			}
			catch
			{
				throw new Exception("Bad stacktrace content in log file: " + _currLog.logResult +  "\n");
			}
			nodeidx = 0;
			diffTracetbl = new DataTable("diffTrace");
			summaryTracetbl = new DataTable("summaryTracetbl");
			MakeDiffTreceTable(diffTracetbl);
			MakeDiffTreceTable(summaryTracetbl);

		    Debug.Assert(_currcallTrace.callTreeView != null);

			string rname = _currcallTrace.MakeName((TreeNode)_currcallTrace.callTreeView.Root);
			Root = new DiffDataNode(rname);
            Debug.Assert(_prevcallTrace.callTreeView != null);
            Root.prevIncl = ((TreeNode)_prevcallTrace.callTreeView.Root).data.bytesAllocated;
		    Debug.Assert(_currcallTrace.callTreeView != null);
			Root.currIncl = ((TreeNode)_currcallTrace.callTreeView.Root).data.bytesAllocated;
			Root.diffIncl = Root.currIncl - Root.prevIncl;
			Root.prevCalls = ((TreeNode)_prevcallTrace.callTreeView.Root).data.numberOfFunctionsCalled;
			Root.currCalls = ((TreeNode)_currcallTrace.callTreeView.Root).data.numberOfFunctionsCalled;
			Root.diffCalls = Root.currCalls - Root.prevCalls;

			Root.nodeId = nodeidx++;
			Root.parentId = -1;
			Root.prevFunId = 0;
			Root.currFunId = 0;
			AddDiffTraceTableRow(diffTracetbl, Root);
			
			BuildDiffTraceTable(Root, (TreeNode)_currcallTrace.callTreeView.Root, (TreeNode)_prevcallTrace.callTreeView.Root);
			this.ds.Tables.Add(diffTracetbl);
			sumnodeidx = 0;
			depth = -1;
			BuildSummaryTable(Root, -1, "parentid = -1");
		    Debug.Assert(Root.allkids != null, "Root.allkids != null");
		    Root = (DiffDataNode)Root.allkids[0];
			this.ds.Tables.Add(summaryTracetbl);

		

		}
		#endregion

		#region build Base table method
		private void BuildBaseData([NotNull] GraphBase gb, CallTreeForm tmpcallTree,  Hashtable htbl, Hashtable FuncExcl, Hashtable TypeExcl)
		{
			Vertex selectedVertex;
			int selectedVertexCount = gb.SelectedVertexCount(out selectedVertex);

		    var n = new datanode();


			try
			{
				foreach (Vertex v in gb.basegraph.vertices.Values)
				{
					if( !v.name.StartsWith("????"))
					{
						if (v.selected || (selectedVertexCount == 0) )
						{
							string nameAndSignature = v.GetNameAndSignature();

                            n.name = nameAndSignature;
							n.incl = FormatSize((int)v.weight);
							n.caller = v.incomingEdges;
							n.callee = v.outgoingEdges;
							n.level = v.level;
							n.excl = 0;
							n.timesBeenCalled= n.timesMakeCalls = 0;
							FillCallAlloc(ref n, v);
						    int id;
						    if(tmpcallTree.LogResult.allocatedHistogram.readNewLog.funcSignatureIdHash.ContainsKey(nameAndSignature))
							{
								n.category = 1;	// func
								id = tmpcallTree.LogResult.callstackHistogram.readNewLog.funcSignatureIdHash[nameAndSignature];
								if(FuncExcl.ContainsKey(nameAndSignature))
								{
									n.excl =  FormatSize((int)FuncExcl[nameAndSignature]);
								}
								if( id > 0 && id <tmpcallTree.CallStats.Length)
								{

									n.timesBeenCalled = (int)tmpcallTree.CallStats[id].timesCalled;
									n.timesMakeCalls = (int)tmpcallTree.CallStats[id].totalFunctionsCalled;
								}
								/*if( id > 0 && CallStats.ContainsKey(id))
								{
									n.timesBeenCalled = int.Parse(((GlobalCallStats)CallStats[id]).timesCalled.ToString());
									n.timesMakeCalls = int.Parse(((GlobalCallStats)CallStats[id]).totalFunctionsCalled.ToString());
								}*/
								if( !htbl.ContainsKey(nameAndSignature))
								{
									htbl.Add(nameAndSignature, n);
								}
							}
							else if(tmpcallTree.LogResult.allocatedHistogram.readNewLog.typeSignatureIdHash.ContainsKey(nameAndSignature))
							{
								n.category = 2;	// type
								id = tmpcallTree.LogResult.allocatedHistogram.readNewLog.typeSignatureIdHash[nameAndSignature];
								if(TypeExcl.ContainsKey(nameAndSignature))
								{
									n.excl =  FormatSize((int)TypeExcl[nameAndSignature]);
								}
								if( id > 0 && id <tmpcallTree.AllocStats.Length)
								{
									n.timesBeenCalled = (int)tmpcallTree.AllocStats[id].timesAllocated;
								}
								/*if(CallStats.ContainsKey(id))
								{
									n.timesBeenCalled = int.Parse(((GlobalCallStats)CallStats[id]).timesCalled.ToString());
								}*/
								if( !htbl.ContainsKey(nameAndSignature))
								{
									typemaxIncl = (typemaxIncl > v.weight) ? typemaxIncl : v.weight;
									htbl.Add(nameAndSignature, n);
								}
							}
							else //if( nameAndSignature == "<root>" || nameAndSignature == "<bottom>")
							{
								if( !htbl.ContainsKey(nameAndSignature))
								{
									maxIncl = v.weight;
									htbl.Add(nameAndSignature, n);
								}
							}

						}
			
					}
				}
			}
			catch
			{
				throw new Exception("Faild on build base data structure \n");
			}
			// max for caculate function/type 9 details 
			if( prevFilter.max == 0)
			{
				prevFilter.max = maxIncl;
				prevTypedeFilter.max = typemaxIncl;
			}
			else
			{
				currFilter.max = maxIncl;
				currTypedeFilter.max = typemaxIncl;
			}
			maxIncl = 0;
			typemaxIncl = 0;
		}
		private void FillCallAlloc(ref datanode n, Vertex v)
		{
			n.calleeAlloc = new Hashtable();
			n.callerAlloc = new Hashtable();
			foreach(Edge edge in v.outgoingEdges.Values)
			{
			    string key = edge.ToVertex.GetNameAndSignature();

                if (!n.calleeAlloc.ContainsKey(key))
				{
					n.calleeAlloc.Add(key, FormatSize((int)edge.weight));
				}
			}
			foreach(Edge edge in v.incomingEdges.Values)
			{
				string key = edge.FromVertex.GetNameAndSignature();

                if (!n.callerAlloc.ContainsKey(key))
				{
					n.callerAlloc.Add(key, FormatSize((int)edge.weight));
				}
			}
		}

		private void BuildBaseDataTable()
		{
			 int id = 0;
			try
			{
				foreach(string nameAndSignature in _prevbasedata.Keys)
				{
				    var cn= new datanode();
					var pn = (datanode)_prevbasedata[nameAndSignature];
					pn.id = id;
					if(_currbasedata.ContainsKey(nameAndSignature))
					{
						cn = (datanode)_currbasedata[nameAndSignature];
					}
					AddBaseTableRow(this.basedatatable, nameAndSignature, pn, cn);
					basedataId.Add(nameAndSignature, id);
					Idbasedata.Add(id, nameAndSignature);
					id++;
				}
				foreach( string CnameAndSignature in _currbasedata.Keys)
				{
					if(! _prevbasedata.ContainsKey(CnameAndSignature))
					{
						var pn= new datanode();
					    var cn = (datanode)_currbasedata[CnameAndSignature];
						cn.id = id;
						AddBaseTableRow(this.basedatatable, CnameAndSignature, pn, cn);
						basedataId.Add(CnameAndSignature, id);
						Idbasedata.Add(id, CnameAndSignature);
						id++;
					}
				}
				ds.Tables.Add(this.basedatatable);
			}
			catch
			{
				throw new Exception("Faild on build base data Tables \n");
			}
		}
		private void AddBaseTableRow(DataTable tmptbl, string name, datanode pn,  datanode cn )
		{
			/*if( (cn.incl - pn.incl == 0) && (cn.excl - pn.excl == 0))
			{
				return;
			}*/
			DataRow tmpRow = tmptbl.NewRow();
			if(pn.name != null)
            {
                tmpRow["id"] = pn.id;
            }
            else 
			{
				tmpRow["id"] = cn.id;
			}

			tmpRow["name"] = name;
			tmpRow["prevIncl"] = pn.incl;
			tmpRow["currIncl"] = cn.incl;
			tmpRow["prevExcl"] = pn.excl;
			tmpRow["currExcl"] = cn.excl;
#if (V_EXEC)
			//tmpRow["diffIncl"] = Convert.ToDouble(string.Format("{0:f2}", (cn.incl - pn.incl)));
			tmpRow["diffIncl"] = Math.Round((cn.incl - pn.incl), 2);
			tmpRow["diffExcl"] =  Math.Round((cn.excl - pn.excl), 2);
			tmpRow["prevChildIncl"] =  Math.Round((pn.incl - pn.excl), 2);
			tmpRow["currChildIncl"] =  Math.Round((cn.incl - cn.excl), 2);
			tmpRow["diffChildIncl"] =  Math.Round(((cn.incl - cn.excl) - (pn.incl - pn.excl)), 2);
#else
			tmpRow["diffIncl"] = cn.incl - pn.incl;
			tmpRow["diffExcl"] = cn.excl - pn.excl;
			tmpRow["prevChildIncl"] = pn.incl - pn.excl;
			tmpRow["currChildIncl"] = cn.incl - cn.excl;
			tmpRow["diffChildIncl"] = (cn.incl - cn.excl) - (pn.incl - pn.excl);
			
#endif
			tmpRow["prevTimesCalled"] = pn.timesBeenCalled;
			tmpRow["currTimesCalled"] = cn.timesBeenCalled;
			tmpRow["prevTimesMakecalls"] = pn.timesMakeCalls;
			tmpRow["currTimesMakecalls"] = cn.timesMakeCalls;
			tmpRow["diffTimesCalled"] = cn.timesBeenCalled - pn.timesBeenCalled;
			tmpRow["diffTimesMakecalls"] = cn.timesMakeCalls - pn.timesMakeCalls;

			/*tmpRow["prevlevel"] = pn.level;
			tmpRow["currlevel"] = cn.level;

			tmpRow["prevcat"] = pn.category;
			tmpRow["currcat"] = cn.category;*/
			
			tmptbl.Rows.Add(tmpRow);
		}
		public void MakeBaseDataTable(DataTable tbl)
		{
		    AddIntColumn(tbl, "id");
		    AddStringColumn(tbl, "name");
		    AddDoubleIntColumn(tbl, "prevIncl");
		    AddDoubleIntColumn(tbl, "currIncl");
		    AddDoubleIntColumn(tbl, "diffIncl");
		    AddDoubleIntColumn(tbl, "prevExcl");
		    AddDoubleIntColumn(tbl, "currExcl");
		    AddDoubleIntColumn(tbl, "diffExcl");
		    AddDoubleIntColumn(tbl, "prevChildIncl");
		    AddDoubleIntColumn(tbl, "currChildIncl");
		    AddDoubleIntColumn(tbl, "diffChildIncl");
		    AddIntColumn(tbl, "prevTimesCalled");
		    AddIntColumn(tbl, "currTimesCalled");
		    AddIntColumn(tbl, "diffTimesCalled");
		    AddIntColumn(tbl, "prevTimesMakecalls");
		    AddIntColumn(tbl, "currTimesMakecalls");
		    AddIntColumn(tbl, "diffTimesMakecalls");

		    AddIntColumn(tbl, "prevlevel");
		    AddIntColumn(tbl, "currlevel");
		    AddIntColumn(tbl, "prevcat");
		    AddIntColumn(tbl, "currcat");
						
		/*	DataColumn[] pk = new DataColumn[1];
			pk[0] = tbl.Columns["name"];
			string tblkey = "PK_" + tbl.TableName;
			tbl.Constraints.Add(new UniqueConstraint(tblkey, pk[0]));
			tbl.PrimaryKey = pk;*/
		}
		
		#endregion


		#region build Caller and callee table
		private void BuildBaseCallTables()
		{
		    try
			{
				foreach(DictionaryEntry de in basedataId)
				{
				    var key = (string)de.Key;
					var id = (int)basedataId[key];
					if(this._prevbasedata.ContainsKey(key))
					{
					    var pn = (datanode) _prevbasedata[key];
					    pn.id = id;
                        BuildCalleerTables(callertbl, id, pn.caller);
                        BuildCalleeTables(calleetbl, id, pn.callee);
					}
					else
					{
						if(_currbasedata.ContainsKey(key))
					    {
					        var cn = (datanode) _currbasedata[key];
					        cn.id = id;
                            BuildCalleerTables(callertbl, id, cn.caller);
                            BuildCalleeTables(calleetbl, id, cn.callee);
					    }
					}
				
				}
				ds.Tables.Add(this.callertbl);
				ds.Tables.Add(this.calleetbl);
			}
			catch
			{
				throw new Exception("Faild on build caller/callee data Tables \n");
			}
		}

	    private void BuildCalleeTables(DataTable tbl, int id, Dictionary<Vertex, Edge> callhash)
	    {
	        foreach (Vertex cv in callhash.Keys)
	        {
	            string key = cv.GetNameAndSignature();

	            if (basedataId.ContainsKey(key))
	            {
	                int calleeid = (int)basedataId[key];
	                AddNamedRow(tbl, id, "calleeid", calleeid);
	            }
	        }
	    }

        private void BuildCalleerTables(DataTable tbl, int id, Dictionary<Vertex, Edge> callhash)
	    {
            foreach (Vertex cv in callhash.Keys)
	        {
	            string key = cv.GetNameAndSignature();

                if (basedataId.ContainsKey(key))
	            {
	                int callerid = (int)basedataId[key];
	                AddNamedRow(tbl, id, "callerid", callerid);
	            }
	        }
	    }

	    private void AddNamedRow(DataTable tmptbl, DoubleInt id, string name, DoubleInt value)
	    {
	        DataRow tmpRow = tmptbl.NewRow();
	        tmpRow["id"] = id;
	        tmpRow[name] = value;
	        tmptbl.Rows.Add(tmpRow);
        }

	    private void MakeCallerTables(DataTable tbl)
	    {
	        AddIntColumn(tbl, "id");
	        AddIntColumn(tbl, "callerid");
        }

	    private void MakeCalleeTables(DataTable tbl)
	    {
	        AddIntColumn(tbl, "id");
	        AddIntColumn(tbl, "calleeid");
        }
	    #endregion

		#region build Caller and callee Contribution table
		private void BuildContributionCalleeTable()
		{
		    datanode pn1;
			datanode cn1;
			try
			{
				foreach(DictionaryEntry de in basedataId)
				{
					datanode pn;
					var cn= new datanode();
					var nameAndSignature = (string)de.Key;
					
					var id = (int)basedataId[nameAndSignature];
					if(this._prevbasedata.ContainsKey(nameAndSignature))
					{
						pn = (datanode)_prevbasedata[nameAndSignature];
					    bool exist;
					    if(_currbasedata.ContainsKey(nameAndSignature))
						{
							cn = (datanode)_currbasedata[nameAndSignature];
							exist = true;
						}
						else
						{
							exist = false;
						}
						var cnnew = new Hashtable();
						foreach(string nameAndSignature1 in pn.calleeAlloc.Keys)
						{
						    cn1 = new datanode();
						    
							if(this._prevbasedata.ContainsKey(nameAndSignature1))
							{
								pn1 = (datanode)_prevbasedata[nameAndSignature1];
								pn1.id = id;
								pn1.incl = FormatSize(int.Parse(pn.calleeAlloc[nameAndSignature1].ToString()));
								if(exist)
								{
									if(this._currbasedata.ContainsKey(nameAndSignature1))
									{
										cn1 = (datanode)_currbasedata[nameAndSignature1];
										cn1.id = id;
										if(cn.calleeAlloc.ContainsKey(nameAndSignature1))
										{
											cn1.incl = FormatSize(int.Parse(cn.calleeAlloc[nameAndSignature1].ToString()));
										}
										cnnew[nameAndSignature1] = 1;
									}
								}
								AddBaseTableRow(this.ContriTocalleetbl, nameAndSignature1, pn1, cn1);
								
							}
						}
						// adding item in new and not in old
						if(exist)
						{
							foreach(string nameAndSignature1 in cn.calleeAlloc.Keys)
							{
								pn1 = new datanode();
								if(!cnnew.ContainsKey(nameAndSignature1))
								{
									cn1 = (datanode)_currbasedata[nameAndSignature1];
									cn1.id = id;
									cn1.incl = FormatSize(int.Parse(cn.calleeAlloc[nameAndSignature1].ToString()));
									AddBaseTableRow(this.ContriTocalleetbl, nameAndSignature1, pn1, cn1);
								}
							}
						}
					}
					
					if(this._currbasedata.ContainsKey(nameAndSignature))
					{
						cn = (datanode)_currbasedata[nameAndSignature];
						pn1= new datanode();
						if(!this._prevbasedata.ContainsKey(nameAndSignature))
						{
							cn1 = (datanode)_currbasedata[nameAndSignature];
							foreach(string nameAndSignature1 in cn.calleeAlloc.Keys)
							{
								if(this._currbasedata.ContainsKey(nameAndSignature1))
								{
									cn1 = (datanode)_currbasedata[nameAndSignature1];
									cn1.id = id;
									cn1.incl = FormatSize(int.Parse(cn.calleeAlloc[nameAndSignature1].ToString()));
									AddBaseTableRow(this.ContriTocalleetbl, nameAndSignature1, pn1, cn1);
								}
								
							}
						}
					}
				}
				ds.Tables.Add(this.ContriTocalleetbl);
			}
			catch
			{
				throw new Exception("Faild on build caller/callee data Tables \n");
			}
		}


		private void BuildContributionCallerTable()
		{
		    datanode pn1;
			datanode cn1;
			try
			{
				foreach(DictionaryEntry de in basedataId)
				{
					datanode pn;
					var cn= new datanode();
					var nameAndSignature = (string)de.Key;
					var id = (int)basedataId[nameAndSignature];
					if(this._prevbasedata.ContainsKey(nameAndSignature))
					{
						pn = (datanode)_prevbasedata[nameAndSignature];
					    bool exist = _currbasedata.ContainsKey(nameAndSignature);
					    if(exist)
						{
							cn = (datanode)_currbasedata[nameAndSignature];
						}
						var cnnew = new Hashtable();
						foreach(Edge edge in pn.caller.Values)
						{
						    cn1 = new datanode();
						    string key = edge.FromVertex.GetNameAndSignature();

                            if (this._prevbasedata.ContainsKey(key))
							{
								pn1 = (datanode)_prevbasedata[key];
								pn1.id = id;
								pn1.incl = FormatSize((int)edge.weight);
								if(exist)
								{
									foreach(Edge edgec in cn.caller.Values)
									{
										if(edgec.FromVertex.name == edge.FromVertex.name && edgec.FromVertex.signature == edge.FromVertex.signature)
										{
											if(this._currbasedata.ContainsKey(key))
											{
												cn1 = (datanode)_currbasedata[key];
												cn1.id = id;
												cn1.incl = FormatSize((int)edgec.weight);
												cnnew[key] = 1;
											}
										}
									}
								}
								AddBaseTableRow(this.ContriTocallertbl, key, pn1, cn1);
								
							}
						}
						// adding item in new and not in old
						if(exist)
						{
							foreach(Edge edgec in cn.caller.Values)
							{
								pn1 = new datanode();
								string key = edgec.FromVertex.GetNameAndSignature();

                                if (!cnnew.ContainsKey(key))
								{
									cn1 = (datanode)_currbasedata[key];
									cn1.id = id;
									cn1.incl = FormatSize((int)edgec.weight);
									AddBaseTableRow(this.ContriTocallertbl, key, pn1, cn1);
								}
							}
						}
					}
					
					if(this._currbasedata.ContainsKey(nameAndSignature))
					{
						cn = (datanode)_currbasedata[nameAndSignature];
						pn1= new datanode();
						if(!this._prevbasedata.ContainsKey(nameAndSignature))
						{
							cn1 = (datanode)_currbasedata[nameAndSignature];
							foreach(Edge edge in cn.caller.Values)
							{
								string key = edge.FromVertex.GetNameAndSignature();

                                if (this._currbasedata.ContainsKey(key))
								{
									cn1 = (datanode)_currbasedata[key];
									cn1.id = id;
									cn1.incl = FormatSize((int)edge.weight);
									AddBaseTableRow(this.ContriTocallertbl, key, pn1, cn1);
								}
								
							}
						}
					}
				}
				ds.Tables.Add(this.ContriTocallertbl);
			}
			catch
			{
				throw new Exception("Faild on build caller/callee data Tables \n");
			}
		}
		#endregion

		#region share used functions

	    private void AddDoubleIntColumn([NotNull] DataTable tbl, [NotNull] string name)
	    {
	        AddColumn(tbl, typeof(DoubleInt), name);
	    }

	    private void AddStringColumn([NotNull] DataTable tbl, [NotNull] string name)
	    {
	        AddColumn(tbl, typeof(string), name);
	    }

        private void AddIntColumn([NotNull] DataTable tbl, [NotNull] string name)
	    {
	        AddColumn(tbl, typeof(int), name);
        }

	    private void AddColumn([NotNull] DataTable tbl, [NotNull] Type type, [NotNull] string name)
	    {
	        var tmpColumn = new DataColumn();
	        tmpColumn.DataType = type;
	        tmpColumn.ColumnName = name;
	        tbl.Columns.Add(tmpColumn);
        }

		private void getDetailFilter(ref DetailFilter df)
		{
			//DataRow[] r = basedatatable.Select("prevIncl = max(prevIncl)");
			//double max = (double)r[0][2];
#if (V_EXEC)
			double max = FormatSize(df.max);
			df.detail01 = Math.Round( (max / detail01D), 2);
			df.detail02 = Math.Round((max / (detail01D - 1)), 2);
			df.detail05 = Math.Round((max / (detail01D - 2)), 2);
			df.detail1 = Math.Round((max / (detail01D - 3)), 2);
			df.detail2 = Math.Round((max / (detail01D - 4)), 2);
			df.detail5 = Math.Round((max / (detail01D - 5)), 2);
			df.detail10 = Math.Round((max / (detail01D - 6)), 2);
#else
			ulong max = df.max;
			df.detail01 = max / detail01D;
			df.detail02 = max / (detail01D - 1);
			df.detail05 = max /(detail01D - 2);
			df.detail1 = max / (detail01D - 3);
			df.detail2 = max / (detail01D - 4);
			df.detail5 = max / (detail01D - 5);
			df.detail10 = max / (detail01D - 6);
#endif
		}

		DoubleInt FormatSize(DoubleInt size)
		{
#if (V_EXEC)
            double w = size;
			w /= 1024;
			return Math.Round(w, 2);
#else
            return size;
#endif
		}
        #endregion

        #region CallTrace - MakeDiffTreceTable, BuildDiffTraceTable
		private void BuildDiffTraceTable(DiffDataNode parent, [CanBeNull] TreeNode currRoot, [CanBeNull] TreeNode prevRoot)
		{
		    var currDKids = new ArrayList();
		    var prevDKids = new ArrayList();

			//get kids
			if(currRoot != null)
			{
			    ArrayList currKids = _currcallTrace.FetchKids(null, currRoot);
			    if(currKids.Count >0)
				{
					currDKids = TransCurrTree(currKids);
				}
			}
			if(prevRoot != null)
			{
			    ArrayList prevKids = _prevcallTrace.FetchKids(null, prevRoot);
			    if(prevKids.Count > 0)
				{
					prevDKids = TransPrevTree(prevKids);
				}
			}
          
			// get diff node
			var diffKids = GetDiffKids(parent, currDKids, prevDKids);

			// recursive for each diff node
			for(int i = 0; i < diffKids.Count; i++)
			{
				BuildDiffTraceTable(diffKids[i] as DiffDataNode, ((DiffDataNode)diffKids[i]).currTreenode as TreeNode, ((DiffDataNode)diffKids[i]).prevTreenode as TreeNode);
				
			}
			

		}
	
		private ArrayList TransCurrTree([NotNull] ArrayList treeNode)
		{
			var diffnodes = new ArrayList();
			int functionId = 0;

		    for( int i = 0; i < treeNode.Count; i++)
			{
				var kidNode = (TreeNode) treeNode[i];
				if(kidNode.data.bytesAllocated >0)
				{
					
					int [] kidStacktrace = _currcallTrace.IndexToStacktrace(kidNode.stackid);
					if (kidNode.nodetype == TreeNode.NodeType.Call)
					{
						functionId = kidStacktrace[ kidStacktrace.Length - 1 ];
					}
					else if (kidNode.nodetype == TreeNode.NodeType.Allocation)
					{
						functionId = kidStacktrace[ 0 ];
					
					}
				
					string name = _currcallTrace.MakeName(kidNode);
					var node = new DiffDataNode(name, (DiffDataNode.NodeType)kidNode.nodetype);
					node.currIncl = kidNode.data.bytesAllocated;
					node.currCalls = kidNode.data.numberOfFunctionsCalled;
					node.currTreenode = kidNode;
					
					switch(node.nodetype)
					{
						case DiffDataNode.NodeType.Allocation:
							node.currFunId = functionId;
							break;

						case DiffDataNode.NodeType.Call:
							node.currFunId = functionId;
							node.mapname = _currcallTrace.names[functionId];
							string sig = _currcallTrace.signatures[functionId];
							if(sig != null)
							{
								node.mapname += " " + sig;
							}
							break;
					}
				
					diffnodes.Add(node);
				}
				
			}

			return diffnodes;
		}

		private ArrayList TransPrevTree([NotNull] ArrayList treeNode)
		{
			var diffnodes = new ArrayList();
			int functionId = 0;

		    for( int i = 0; i < treeNode.Count; i++)
			{
				var kidNode = (TreeNode) treeNode[i];
				if(kidNode.data.bytesAllocated >0)
				{
					
					int [] kidStacktrace = _prevcallTrace.IndexToStacktrace(kidNode.stackid);
					if (kidNode.nodetype == TreeNode.NodeType.Call)
					{
						functionId = kidStacktrace[ kidStacktrace.Length - 1 ];
					}
					else if (kidNode.nodetype == TreeNode.NodeType.Allocation)
					{
						functionId = kidStacktrace[ 0 ];
					
					}
				
					string name = _prevcallTrace.MakeName(kidNode);
					var node = new DiffDataNode(name, (DiffDataNode.NodeType)kidNode.nodetype);
					node.prevIncl = kidNode.data.bytesAllocated;
					node.prevCalls = kidNode.data.numberOfFunctionsCalled;
					node.prevTreenode = kidNode;
					
					switch(node.nodetype)
					{
						case DiffDataNode.NodeType.Allocation:
							node.prevFunId = functionId;
							break;

						case DiffDataNode.NodeType.Call:
							node.prevFunId= functionId;
							node.mapname = _prevcallTrace.names[functionId];
							string sig = _prevcallTrace.signatures[functionId];
							if(sig != null)
							{
								node.mapname += " " + sig;
							}
							break;
					}
					diffnodes.Add(node);
				
				}
			}

			return diffnodes;
		}
		
		
		private ArrayList GetDiffKids(DiffDataNode parent, ArrayList currKids, ArrayList prevKids)
		{
			var curr = new ArrayList();
			var curr_inclOfNode = new Hashtable();
			var prev = new ArrayList();
			var prev_inclOfNode = new Hashtable();

			var diffnodes = new ArrayList();
			for(int i = 0; i < currKids.Count; i++)
			{
				if( !((DiffDataNode)currKids[i]).marked)
				{
					var node = new DiffDataNode( ((DiffDataNode)currKids[i]).name);
					int idx = CurrExactMatchIndex(prevKids, (DiffDataNode) currKids[i]);
					if(idx >=0)
					{
						node.currFunId = ((DiffDataNode)currKids[i]).currFunId;
						node.prevFunId = ((DiffDataNode)prevKids[idx]).prevFunId;
						node.mapname = ((DiffDataNode)currKids[i]).mapname;
						node.currIncl = ((DiffDataNode)currKids[i]).currIncl;
						node.prevIncl = ((DiffDataNode)prevKids[idx]).prevIncl;
						node.diffIncl = node.currIncl - node.prevIncl;
						node.currCalls = ((DiffDataNode)currKids[i]).currCalls;
						node.prevCalls = ((DiffDataNode)prevKids[idx]).prevCalls;
						node.diffCalls = node.currCalls - node.prevCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)currKids[i]).currTreenode;
						node.prevTreenode = ((DiffDataNode)prevKids[idx]).prevTreenode;
						node.nodetype = ((DiffDataNode)currKids[i]).nodetype;

						((DiffDataNode)currKids[i]).marked = true;
						((DiffDataNode)prevKids[idx]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						
						}
					}
					else
					{
						long incl = ((DiffDataNode)currKids[i]).currIncl;
						curr_inclOfNode[currKids[i]] = incl; 
						//string nm = ((DiffDataNode)currKids[i]).mapname;
						//curr_inclOfNode[currKids[i]] = nm; 
						curr.Add(currKids[i]);
					}
				}
			}

			for(int i = 0; i < prevKids.Count; i++)
			{
				if( !((DiffDataNode)prevKids[i]).marked)
				{
					var node = new DiffDataNode( ((DiffDataNode)prevKids[i]).name);
					int idx = PrevExactMatchIndex(currKids, (DiffDataNode) prevKids[i]);
					if(idx >=0)
					{
						node.currFunId = ((DiffDataNode)currKids[idx]).currFunId;
						node.prevFunId = ((DiffDataNode)prevKids[i]).prevFunId;
						node.mapname = ((DiffDataNode)currKids[idx]).mapname;
						node.currIncl = ((DiffDataNode)currKids[idx]).currIncl;
						node.prevIncl = ((DiffDataNode)prevKids[i]).prevIncl;
						node.diffIncl = node.currIncl - node.prevIncl;
						node.currCalls = ((DiffDataNode)currKids[idx]).currCalls;
						node.prevCalls = ((DiffDataNode)prevKids[i]).prevCalls;
						node.diffCalls = node.currCalls - node.prevCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)currKids[idx]).currTreenode;
						node.prevTreenode = ((DiffDataNode)prevKids[i]).prevTreenode;
						node.nodetype = ((DiffDataNode)prevKids[i]).nodetype;

						((DiffDataNode)currKids[idx]).marked = true;
						((DiffDataNode)prevKids[i]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						
						}
					}
					else
					{
						long incl = ((DiffDataNode)prevKids[i]).prevIncl;
						prev_inclOfNode[prevKids[i]] = incl; 
						//string nm = ((DiffDataNode)prevKids[i]).mapname;
						//prev_inclOfNode[prevKids[i]] = nm;
						prev.Add(prevKids[i]);
					}
				}
			}
			curr.Sort(new CompareIncl(curr_inclOfNode));
			prev.Sort(new CompareIncl(prev_inclOfNode));
			for(int i = 0; i < curr.Count; i++)
			{
				
				if( !((DiffDataNode)curr[i]).marked)
				{
					var node = new DiffDataNode( ((DiffDataNode)curr[i]).name);
					int idx = FirstMatchIndex(prevKids, (DiffDataNode) curr[i]);
					if(idx >=0)
					{
						node.currFunId = ((DiffDataNode)curr[i]).currFunId;
						node.prevFunId = ((DiffDataNode)prevKids[idx]).prevFunId;
						node.mapname = ((DiffDataNode)curr[i]).mapname;
						node.currIncl = ((DiffDataNode)curr[i]).currIncl;
						node.prevIncl = ((DiffDataNode)prevKids[idx]).prevIncl;
						node.diffIncl = node.currIncl - node.prevIncl;
						node.currCalls = ((DiffDataNode)curr[i]).currCalls;
						node.prevCalls = ((DiffDataNode)prevKids[idx]).prevCalls;
						node.diffCalls = node.currCalls - node.prevCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)curr[i]).currTreenode;
						node.prevTreenode = ((DiffDataNode)prevKids[idx]).prevTreenode;
						node.nodetype = ((DiffDataNode)curr[i]).nodetype;

						((DiffDataNode)curr[i]).marked = true;
						((DiffDataNode)prevKids[idx]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						
						}
					}
					else
					{
						node.currFunId = ((DiffDataNode)curr[i]).currFunId;
						node.mapname = ((DiffDataNode)curr[i]).mapname;
						node.currIncl = ((DiffDataNode)curr[i]).currIncl;
						node.prevIncl = 0;
						node.diffIncl = node.currIncl;
						node.currCalls = ((DiffDataNode)curr[i]).currCalls;
						node.prevCalls = 0;
						node.diffCalls = node.currCalls;
						
						node.nodeId = nodeidx;
						node.parentId = parent.nodeId;
						node.parentname = parent.name;
						node.currTreenode = ((DiffDataNode)curr[i]).currTreenode;
						node.nodetype = ((DiffDataNode)curr[i]).nodetype;
						((DiffDataNode)curr[i]).marked = true;
						if(node.diffIncl != 0)
						{
							diffnodes.Add(node);
							AddDiffTraceTableRow(diffTracetbl, node);
							nodeidx++;
						}
					}
				}
				
			}

			for(int i = 0; i < prev.Count; i++)
			{
				if(!((DiffDataNode)prev[i]).marked)
				{
					var node = new DiffDataNode( ((DiffDataNode)prev[i]).name, ((DiffDataNode)prev[i]).nodetype);
					
					// prev not exist in curr
					node.prevFunId = ((DiffDataNode)prev[i]).prevFunId;
					node.mapname = ((DiffDataNode)prev[i]).mapname;
					node.currIncl = 0;
					node.prevIncl = ((DiffDataNode)prev[i]).prevIncl;
					node.diffIncl = -node.prevIncl;
					node.currCalls = 0;
					node.prevCalls = ((DiffDataNode)prev[i]).prevCalls;
					node.diffCalls = -node.prevCalls;
					
					node.nodeId = nodeidx;
					node.parentId = parent.nodeId;
					node.parentname = parent.name;
					node.prevTreenode = ((DiffDataNode)prev[i]).prevTreenode;

					((DiffDataNode)prev[i]).marked = true;
					if(node.diffIncl != 0)
					{
						diffnodes.Add(node);
						AddDiffTraceTableRow(diffTracetbl, node);
						nodeidx++;
					}
				}
				
			}
			for(int i = 0; i < currKids.Count; i++)
			{
				((DiffDataNode)currKids[i]).marked = false;
			}
			for(int i = 0; i < prevKids.Count; i++)
			{
				((DiffDataNode)prevKids[i]).marked = false;
			}

			return diffnodes;
		}
	
		

		private int CurrExactMatchIndex(ArrayList nodelst, DiffDataNode node)
		{
			for(int i = 0; i < nodelst.Count; i++)
			{
				if( ((DiffDataNode)nodelst[i]).name.Equals(node.name) && 
					!((DiffDataNode)nodelst[i]).marked &&
					(node.currIncl - ((DiffDataNode)nodelst[i]).prevIncl) == 0)
				{
					return i;
				}
			}
			return -1;
		}
		private int PrevExactMatchIndex(ArrayList nodelst, DiffDataNode node)
		{
			for(int i = 0; i < nodelst.Count; i++)
			{
				if( ((DiffDataNode)nodelst[i]).name.Equals(node.name) && 
					!((DiffDataNode)nodelst[i]).marked &&
					(node.prevIncl - ((DiffDataNode)nodelst[i]).currIncl) == 0)
				{
					return i;
				}
			}
			return -1;
		}
	/*	private int FirstMatchIndex(ArrayList nodelst, DiffDataNode node)
		{
			for(int i = 0; i < nodelst.Count; i++)
			{
				if( ((DiffDataNode)nodelst[i]).name.Equals(node.name) && 
					!((DiffDataNode)nodelst[i]).marked )
					return i;
			}
			return -1;
		}*/
		private int FirstMatchIndex(ArrayList nodelst, DiffDataNode node)
		{
			
			int idx = -1;
			long savedalloc = long.MaxValue;

		    for(int i = 0; i < nodelst.Count; i++)
			{
				if( ((DiffDataNode)nodelst[i]).name.Equals(node.name) && 
					!((DiffDataNode)nodelst[i]).marked )
				{
				    long alloc = Math.Abs(node.currIncl - ((DiffDataNode)nodelst[i]).prevIncl);
				    if(alloc < savedalloc)
					{
						idx = i;
						savedalloc = alloc;
						continue;
					}
				}
			}
			
			return idx;
		}
		
		#endregion

		#region Summary table
		internal void RefreshCallTreeNodes([NotNull] DiffDataNode node)
		{
			node.IsExpanded = false;
		    Debug.Assert(node.allkids != null, "node.allkids != null");
		    for(int i = 0; i < node.allkids.Count; i++)
			{
				RefreshCallTreeNodes((DiffDataNode) node.allkids[i]);
			}

		}
		internal void GetAllKids([NotNull] DiffDataNode root, string filter)
		{
			DataRow[] rKids = summaryTracetbl.Select(filter, "name asc");
			if(rKids.Length > 0)
			{
				root.HasKids = true;
				root.depth = 0;
			}
			Debug.Assert(root.allkids != null, "root.allkids != null");
			for(int i = 0; i < rKids.Length; i++)
			{
				DiffDataNode kidNode = Row2Node(rKids[i]);
			    root.allkids.Add(kidNode);
				
			}
		}
		private void BuildSummaryTable([NotNull] DiffDataNode parent, int parentId, string filter)
		{
			depth++;
			parent.depth = depth;
		    Debug.Assert(parent.allkids != null, "parent.allkids != null");
		    parent.allkids.Clear();
			parent.HasKids = false;
			var kidSum = new Hashtable();

		    DataRow[] kidsRows = diffTracetbl.Select(filter);
			for(int i = 0; i < kidsRows.Length; i++)
			{
				DiffDataNode sumNode = Row2Node(kidsRows[i]);
				string name = sumNode.mapname;
				if(kidSum.ContainsKey(name))
				{
					var updateNode = (DiffDataNode) kidSum[name];
					updateNode.prevIncl += sumNode.prevIncl;
					updateNode.currIncl += sumNode.currIncl;
					updateNode.diffIncl = updateNode.currIncl - updateNode.prevIncl;
					if(sumNode.prevIncl != 0)
					{
						updateNode.prevCalls++;
					}
					if(sumNode.currIncl != 0)
					{
						updateNode.currCalls++;
					}
					updateNode.diffCalls = updateNode.currCalls - updateNode.prevCalls;
				    Debug.Assert(updateNode.allkids != null, "updateNode.allkids != null");
				    updateNode.allkids.Add(sumNode.nodeId);
					updateNode.HasKids = true;
					
				}
				else
				{
					if(sumNode.prevIncl != 0)
					{
						sumNode.prevCalls = 1;
					}
					if(sumNode.currIncl != 0)
					{
						sumNode.currCalls = 1;
					}
					sumNode.parentId = parentId;
				    Debug.Assert(sumNode.allkids != null, "sumNode.allkids != null");
				    sumNode.allkids.Add(sumNode.nodeId);
					sumNode.diffIncl = sumNode.currIncl - sumNode.prevIncl;
					sumNode.diffCalls = sumNode.currCalls - sumNode.prevCalls;
					kidSum.Add(name, sumNode);
					sumNode.HasKids = false;
					sumNode.depth = depth;
					sumNode.nodeId = sumnodeidx;
					sumnodeidx++;
				}
				
			}
			if(kidSum.Count > 0)
			{
				if(parent.nodetype == DiffDataNode.NodeType.Call)
				{
					parent.HasKids = true;
				}
				string diffkey = parent.mapname + parent.prevIncl + parent.currIncl + parent.diffIncl + parent.prevFunId + parent.currFunId;
				//string diffkey = parent.mapname + parent.diffIncl + parent.prevFunId + parent.currFunId;
				if(!diffCallTreeNodes.ContainsKey(diffkey))
				{
					diffCallTreeNodes.Add(diffkey, parent);
				}
				
			}
						

			foreach(string key in kidSum.Keys)
			{
				var sumNode = (DiffDataNode) kidSum[key];
				if(! (sumNode.diffIncl == 0))
				{
					parent.allkids.Add(sumNode);
                    AddDiffTraceTableRow(summaryTracetbl, sumNode);
				}
				string kidFilter = getFilter(sumNode.allkids);
				BuildSummaryTable(sumNode, sumNode.nodeId,kidFilter);
			}
			
			depth--;
			
		}

	
		private string getFilter(ArrayList kids)
		{
			string filter = "parentId in (";
			if(kids.Count > 1)
			{
				for(int i = 0; i < kids.Count-1; i++)
				{
					filter += kids[i].ToString() + ",";
				}
				filter += kids[kids.Count-1].ToString();
			}
			else if(kids.Count == 1)
			{
				filter += kids[0].ToString();
			}
			filter += ")";
			return filter;
		}
		internal DiffDataNode Row2Node(DataRow r)
		{
			string name = r[idx_name].ToString();
			int nodetype = int.Parse(r[idx_type].ToString());
		    DiffDataNode.NodeType nodeType;
			if(nodetype == 0)
			{
				nodeType = DiffDataNode.NodeType.Call;
			}
			else if(nodetype == 1)
			{
			    nodeType = DiffDataNode.NodeType.Allocation;
			}
			else
			{
			    nodeType = DiffDataNode.NodeType.AssemblyLoad;
			}
			var node = new DiffDataNode(name, nodeType); 
			
			node.mapname = r[idx_mapname].ToString();
				
			node.prevIncl = int.Parse(r[idx_prevIncl].ToString());
			node.currIncl = int.Parse(r[idx_currIncl].ToString());
			node.diffIncl = int.Parse(r[idx_diffIncl].ToString());
			node.prevFunId = int.Parse(r[idx_prevFunid].ToString());
			node.currFunId = int.Parse(r[idx_currFunid].ToString());
			node.prevCalls = int.Parse(r[idx_prevCalls].ToString());
			node.currCalls = int.Parse(r[idx_currCalls].ToString());
			node.diffCalls = int.Parse(r[idx_diffCalls].ToString());
				
			node.nodeId = int.Parse(r[idx_id].ToString());
			node.parentId = int.Parse(r[idx_parentid].ToString());
			node.depth = int.Parse(r[idx_depth].ToString());
			return node;
		}

		private void MakeDiffTreceTable([NotNull] DataTable tbl)
		{
		    AddIntColumn(tbl, "parentid");
		    AddStringColumn(tbl, "parentname");
		    AddIntColumn(tbl, "id");
		    AddStringColumn(tbl, "name");
		    AddStringColumn(tbl, "mapname");

		    AddIntColumn(tbl, "prevIncl");
		    AddIntColumn(tbl, "currIncl");
		    AddIntColumn(tbl, "diffIncl");
		    AddIntColumn(tbl, "prevCalls");
		    AddIntColumn(tbl, "currCalls");
		    AddIntColumn(tbl, "diffCalls");
		    AddIntColumn(tbl, "prevFunId");
		    AddIntColumn(tbl, "currFunId");
		    AddIntColumn(tbl, "nodetype");
		    AddIntColumn(tbl, "depth");
			
			
			

			tbl.Columns["parentid"].DefaultValue = -1;
			tbl.Columns["parentname"].DefaultValue = "";
			tbl.Columns["id"].DefaultValue = -1;
			tbl.Columns["name"].DefaultValue = "";
			tbl.Columns["mapname"].DefaultValue = "";
			tbl.Columns["prevIncl"].DefaultValue = 0;
			tbl.Columns["currIncl"].DefaultValue = 0;
			tbl.Columns["diffIncl"].DefaultValue = 0;
			tbl.Columns["prevCalls"].DefaultValue = 0;
			tbl.Columns["currCalls"].DefaultValue = 0;
			tbl.Columns["diffCalls"].DefaultValue = 0;
			tbl.Columns["prevFunId"].DefaultValue = -1;
			tbl.Columns["currFunId"].DefaultValue = -1;
			tbl.Columns["nodetype"].DefaultValue = -1;
			tbl.Columns["depth"].DefaultValue = 0;
			

			tbl.Columns["diffIncl"].Expression = "currIncl - prevIncl";
			tbl.Columns["diffCalls"].Expression = "currCalls - prevCalls";
			//tbl.Columns["mapname"].Expression = "name";
		}
		private void AddDiffTraceTableRow(DataTable tmptbl, DiffDataNode node)
		{			
			DataRow tmpRow = tmptbl.NewRow();

			tmpRow["parentid"] = node.parentId;
			tmpRow["id"] = node.nodeId;
			tmpRow["parentname"] = node.parentname;
			tmpRow["name"] = node.name;
			tmpRow["mapname"] = node.mapname;
			tmpRow["prevIncl"] = node.prevIncl;
			tmpRow["currIncl"] = node.currIncl;
			tmpRow["prevCalls"] = node.prevCalls;
			tmpRow["currCalls"] = node.currCalls;
			tmpRow["prevFunId"] = node.prevFunId;
			tmpRow["currFunId"] = node.currFunId;
			if(node.nodetype == DiffDataNode.NodeType.Call)
			{
				tmpRow["nodetype"] = 0;
			}
			else if(node.nodetype == DiffDataNode.NodeType.Allocation)
			{
				tmpRow["nodetype"] = 1;
			}
			else if(node.nodetype == DiffDataNode.NodeType.AssemblyLoad)
			{
				tmpRow["nodetype"] = 2;
			}
			tmpRow["depth"] = node.depth;
			tmptbl.Rows.Add(tmpRow);
			
		}
		#endregion
	

		#region EXCLUSIVE
		private void ReadFile(CallTreeForm callTrace, string fileName, Hashtable FuncExcl, Hashtable TypeExcl)
		{
			var funcCalled = new Hashtable();
			var TypeAlloc = new Hashtable();
		    ProgressForm progressForm = null;
			try
			{
				/* log parser code (straight from the ReadNewLog.cs) */
				Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
				r = new StreamReader(s);

				progressForm = new ProgressForm();
				progressForm.Text = "Preparing call tree view";
				progressForm.Visible = true;
				progressForm.setProgress(0);
				progressForm.TopMost = false;
				int maxProgress = (int)(r.BaseStream.Length/1024);
				progressForm.setMaximum(maxProgress);

				buffer = new byte[4096];
				bufPos = 0;
				bufLevel = 0;
				line = 1;
				var sb = new StringBuilder();
				c = ReadChar();

			    string assemblyName = null;
				int threadid = 0;
			    int stackid = 0;
			    TreeNode.NodeType nodetype = TreeNode.NodeType.Call;

				while (c != -1)
				{
					bool found = false;
					if ((line % 1024) == 0)
					{
						int currentProgress = (int)(pos/1024);
						if (currentProgress <= maxProgress)
						{
							progressForm.setProgress(currentProgress);
							//Application.DoEvents();
						}
					}
					lastLineStartPos = pos-1;
					switch (c)
					{
						case    -1:
							break;
				
							// 'A' with thread identifier
						case    '!':
						{
							found = true;
							c = ReadChar();
						    // ReSharper disable once RedundantAssignment
							threadid = ReadInt();
							ReadInt();
							stackid = ReadInt();
							nodetype = TreeNode.NodeType.Allocation;
							if (c == -1)	{found = false;}
							break;
			
						}

						case    'C':
						case    'c':
						{
							found = true;
							c = ReadChar();
							nodetype = TreeNode.NodeType.Call;
						    // ReSharper disable once RedundantAssignment
							threadid = ReadInt();
							stackid = ReadInt();
							if (c == -1)	{found = false;}
							break;
						}

						
						case 'y':
						case 'Y':
						{
							found = true;
							c = ReadChar();
							nodetype = TreeNode.NodeType.AssemblyLoad;
						    // ReSharper disable once RedundantAssignment
							threadid = ReadInt();
							/* int assemblyId = */ ReadInt();

							while (c == ' ' || c == '\t')
							{
								c = ReadChar();
							}
							sb.Length = 0;
							while (c > ' ')
							{
								sb.Append((char)c);
								c = ReadChar();
							}
						    // ReSharper disable once RedundantAssignment
							assemblyName = sb.ToString();
							break;
						}


						default:
						{
							// just ignore the unknown
							while(c != '\n' && c != '\r')
							{
								c = ReadChar();
							}
							break;
						}
					}
					while (c == ' ' || c == '\t')
                    {
                        c = ReadChar();
                    }

                    if (c == '\r')
                    {
                        c = ReadChar();
                    }

                    if (c == '\n')
					{
						c = ReadChar();
						line++;
					}
					if(!found)
					{
						continue;
					}

				    string typename = null;

					int[] stacktrace = callTrace.IndexToStacktrace(stackid);
					int functionId = (nodetype != TreeNode.NodeType.AssemblyLoad ? stacktrace[stacktrace.Length - 1] : 0);
					switch(nodetype)
					{
						case TreeNode.NodeType.Allocation:
					    {
					        string name;
					        if ((functionId < callTrace.LogResult.callstackHistogram.readNewLog.funcName.Length) &&
					            ((name = callTrace.LogResult.callstackHistogram.readNewLog.funcName[functionId]) != null))
					        {
					            if (callTrace.LogResult.callstackHistogram.readNewLog.funcSignature[functionId] != null)
					            {
					                name += " " + callTrace.LogResult.callstackHistogram.readNewLog.funcSignature[functionId];
					            }
					        }
					        else
					        {
					            name = "NATIVE FUNCTION ( UNKNOWN ARGUMENTS )";
					        }

					        // function Excl							
					        if (FuncExcl.ContainsKey(name))
					        {
					            int alloc = (int) FuncExcl[(string) name];
					            alloc += stacktrace[1];
					            FuncExcl[name] = alloc;
					        }
					        else
					        {
					            FuncExcl.Add(name, stacktrace[1]);
					        }

					        // Type Excl
					        if (stacktrace[0] >= 0 && stacktrace[0] <
					            callTrace.LogResult.callstackHistogram.readNewLog.typeName.Length)
					        {
					            typename = callTrace.LogResult.callstackHistogram.readNewLog.typeName[stacktrace[0]];
					        }
					        if (typename == null)
					        {
					            typename = "NATIVE FUNCTION ( UNKNOWN ARGUMENTS )";
					        }

					        if (TypeExcl.ContainsKey(typename))
					        {
					            int alloc = (int) TypeExcl[(string) typename];
					            alloc += stacktrace[1];
					            TypeExcl[typename] = alloc;
					        }
					        else
					        {
					            TypeExcl.Add(typename, stacktrace[1]);
					        }

					        string key;
					        // Type Allocated by Excl
					        if (name != "NATIVE FUNCTION ( UNKNOWN ARGUMENTS )")
					        {
					            key = typename + "|" + functionId;
					        }
					        else
					        {
					            key = typename + "|" + 0;
					        }

					        if (TypeAlloc.ContainsKey(key))
					        {
					            int alloc = (int) TypeAlloc[key];
					            alloc += stacktrace[1];
					            TypeAlloc[key] = alloc;
					        }
					        else
					        {
					            TypeAlloc.Add(key, stacktrace[1]);
					        }

					        break;
					    }
					    case TreeNode.NodeType.Call:
					    {
					        if (funcCalled.ContainsKey(functionId))
					        {
					            int calls = (int) funcCalled[functionId] + 1;
					            ;
					            funcCalled[functionId] = calls;
					        }
					        else
					        {
					            funcCalled.Add(functionId, 1);
					        }
					        break;
					    }
					}
				}

			}
			catch (Exception)
			{
				throw new Exception(string.Format("Bad format in log file {0} line {1}", fileName, line));
			}

			finally
			{
				progressForm.Visible = false;
				progressForm.Dispose();
				if (r != null)
                {
                    r.Close();
                }
            }
		}
		internal int ReadChar()
		{
			pos++;
			if (bufPos < bufLevel)
            {
                return buffer[bufPos++];
            }
            else
            {
                return FillBuffer();
            }
        }
		
		int ReadInt()
		{
			while (c == ' ' || c == '\t')
            {
                c = ReadChar();
            }

            bool negative = false;
			if (c == '-')
			{
				negative = true;
				c = ReadChar();
			}
			if (c >= '0' && c <= '9')
			{
				int value = 0;
				if (c == '0')
				{
					c = ReadChar();
					if (c == 'x' || c == 'X')
                    {
                        value = ReadHex();
                    }
                }
				while (c >= '0' && c <= '9')
				{
					value = value*10 + c - '0';
					c = ReadChar();
				}

				if (negative)
                {
                    value = -value;
                }

                return value;
			}
			else
			{
				return Int32.MinValue;
			}
		}
		int FillBuffer()
		{
			bufPos = 0;
			bufLevel = r.BaseStream.Read(buffer, 0, buffer.Length);
			if (bufPos < bufLevel)
            {
                return buffer[bufPos++];
            }
            else
            {
                return -1;
            }
        }
		int ReadHex()
		{
			int value = 0;
			while (true)
			{
				c = ReadChar();
				int digit = c;
				if (digit >= '0' && digit <= '9')
                {
                    digit -= '0';
                }
                else if (digit >= 'a' && digit <= 'f')
                {
                    digit -= 'a' - 10;
                }
                else if (digit >= 'A' && digit <= 'F')
                {
                    digit -= 'A' - 10;
                }
                else
                {
                    return value;
                }

                value = value*16 + digit;
			}
		}


		#endregion

	}
}
