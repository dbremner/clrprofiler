namespace CLRProfiler
{
    public partial class AllocationDiff 
	{

        // caller and callee tables node
	    private struct callnode
		{
		    public int id { get; set; }

		    public int callerid { get; set; }

		    public int calleeid { get; set; }
		}
	}
}
