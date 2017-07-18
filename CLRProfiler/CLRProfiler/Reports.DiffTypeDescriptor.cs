using System;

namespace CLRProfiler
{
    public partial class Reports
	{
        class DiffTypeDescriptor : IComparable
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
        }
	}
}
