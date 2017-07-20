// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Drawing;

namespace CLRProfiler
{
    public partial class ViewByAddressForm : System.Windows.Forms.Form
    {
        private class TypeDesc : IComparable
        {
            internal readonly string typeName;
            internal readonly int typeIndex;
            internal ulong totalSize;
            internal ulong selectedSize;
            internal double percentage;
            internal double selectedPercentage;
            internal Color[] colors;
            internal Brush[] brushes;
            internal Pen[] pens;
            internal int selected;
            internal Rectangle rect;

            internal TypeDesc(int typeIndex, string typeName)
            {
                this.typeIndex = typeIndex;
                this.typeName = typeName;
            }

            public int CompareTo(Object o)
            {
                var t = (TypeDesc)o;
                if (t.selectedSize != this.selectedSize)
                {
                    if (t.selectedSize < this.selectedSize)
                    {
                        return -1;
                    }
                    else
                    {
                        return 1;
                    }
                }
                if (t.totalSize < this.totalSize)
                {
                    return -1;
                }
                else if (t.totalSize > this.totalSize)
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
