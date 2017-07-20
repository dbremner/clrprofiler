namespace CLRProfiler
{
    internal partial class LiveObjectTable
    {
        private partial class IntervalTable
        {
            private class Interval
            {
                internal ulong loAddr;
                internal ulong hiAddr;
                internal readonly int generation;
                internal Interval next;
                internal bool hadRelocations;
                internal bool justHadGc;

                internal Interval(ulong loAddr, ulong hiAddr, int generation, Interval next)
                {
                    this.loAddr = loAddr;
                    this.hiAddr = hiAddr;
                    this.generation = generation;
                    this.next = next;
                }
            }
        }
    }
}