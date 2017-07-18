using System;

namespace CLRProfiler
{
    public partial class Reports
	{
        class TypeDescriptor : IComparable
        {
            internal readonly int[] size;
            internal readonly int[] count;
            internal readonly int typeIndex;

            internal TypeDescriptor(int typeIndex, int slotCount)
            {
                this.typeIndex = typeIndex;
                this.size = new int[slotCount];
                this.count = new int[slotCount];
            }

            public int CompareTo(Object o)
            {
                var that = (TypeDescriptor)o;
                if (that.size[0] < this.size[0])
                {
                    return -1;
                }
                else if (that.size[0] > this.size[0])
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
