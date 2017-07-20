namespace CLRProfiler
{
    internal sealed partial class LiveObjectTable
    {
        internal struct LiveObject
        {
            internal ulong id;
            internal uint size;
            internal int typeIndex;
            internal int typeSizeStacktraceIndex;
            internal int allocTickIndex;
        }
    }
}