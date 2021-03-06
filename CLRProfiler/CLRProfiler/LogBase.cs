using System;
using System.IO;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for LogBase.
	/// </summary>
	public sealed class LogBase
	{
		#region private data member
		private readonly long logFileStartOffset;
		private readonly long logFileEndOffset;
	    private ReadNewLog log;
		#endregion

		#region public member
		internal ReadLogResult logResult;
		#endregion
		
		public LogBase()
		{
			logFileStartOffset = 0;
			logFileEndOffset = long.MaxValue;
		}
		#region public property methods
		public string LogFileName { get; set; }
	    #endregion
		#region public methods
		public void readLogFile()
		{
			log = new ReadNewLog(LogFileName);
			logResult = GetLogResult();
			log.ReadFile(logFileStartOffset, logFileEndOffset, logResult);
			
		}
		#endregion

		#region private methods
		// from form1.cs
		internal ReadLogResult GetLogResult()
		{
			logResult = new ReadLogResult();
			logResult.liveObjectTable = new LiveObjectTable(log);
			logResult.sampleObjectTable = new SampleObjectTable(log);
			logResult.allocatedHistogram = new Histogram(log);
			logResult.callstackHistogram = new Histogram(log);
			logResult.relocatedHistogram = new Histogram(log);
			logResult.objectGraph = new ObjectGraph(log, 0);
			logResult.functionList = new FunctionList(log);
			logResult.hadCallInfo = logResult.hadAllocInfo = false;

			// We may just have turned a lot of data into garbage - let's try to reclaim the memory
			GC.Collect();
			return logResult;
		}
		#endregion
	}
}
