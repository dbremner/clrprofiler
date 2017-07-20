using System;

namespace CLRProfiler
{
    public static partial class Reports
	{
	    private class TypeDescriptor : IComparable, IComparable<TypeDescriptor>
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

            public int CompareTo(TypeDescriptor other)
            {
                var that = other;
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
