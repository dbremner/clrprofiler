using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal sealed class ReadLogResult
    {
        internal Histogram allocatedHistogram;
        internal Histogram relocatedHistogram;
        internal Histogram callstackHistogram;
        internal Histogram finalizerHistogram;
        internal Histogram criticalFinalizerHistogram;
        internal Histogram[] heapDumpHistograms;
        internal Histogram createdHandlesHistogram;
        internal Histogram destroyedHandlesHistogram;
        internal LiveObjectTable liveObjectTable;
        internal SampleObjectTable sampleObjectTable;
        internal ObjectGraph objectGraph;
        internal ObjectGraph requestedObjectGraph; // accomodate more than one objectGraph
        internal FunctionList functionList;
        internal bool hadAllocInfo, hadCallInfo;
        internal Dictionary<ulong, HandleInfo> handleHash;
    }
}