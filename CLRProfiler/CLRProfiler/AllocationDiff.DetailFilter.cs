//#define V_EXEC	

#if V_EXEC
using DoubleInt = double;
using DoubleUInt64 = double;
#else
using DoubleUInt64 = System.UInt64;
#endif

namespace CLRProfiler
{
    public sealed partial class AllocationDiff 
	{
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
	}
}
