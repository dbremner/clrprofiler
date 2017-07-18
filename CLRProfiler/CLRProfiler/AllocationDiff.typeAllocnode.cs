#if V_EXEC
using DoubleInt = double;
using DoubleUInt64 = double;
#else
using DoubleInt = System.Int32;
#endif

namespace CLRProfiler
{
    public partial class AllocationDiff 
	{
        // typeAllocation table node
        struct typeAllocnode
		{
			public int typeid { get; set; }

		    public int funcid { get; set; }
            
            public DoubleInt allocmem { get; set; }
			
		}
	}
}
