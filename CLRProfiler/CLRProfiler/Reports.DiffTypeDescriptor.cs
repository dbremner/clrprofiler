using System;

namespace CLRProfiler
{
    public static partial class Reports
	{
	    private sealed class DiffTypeDescriptor : IComparable, IComparable<DiffTypeDescriptor>
        {
            internal int aSize;
            internal int aCount;
            internal int bSize;
            internal int bCount;
            internal int diffSize;
            internal int diffCount;
            internal readonly int typeIndex;

            internal DiffTypeDescriptor(int typeIndex)
            {
                this.typeIndex = typeIndex;
            }

            public int CompareTo(Object o)
            {
                var that = (DiffTypeDescriptor)o;
                if (that.diffSize < this.diffSize)
                {
                    return -1;
                }
                else if (that.diffSize > this.diffSize)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }

            public int CompareTo(DiffTypeDescriptor other)
            {
                var that = other;
                if (that.diffSize < this.diffSize)
                {
                    return -1;
                }
                else if (that.diffSize > this.diffSize)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
            }
        }
	}
}
