﻿// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Drawing;

namespace CLRProfiler
{
    public sealed partial class TimeLineViewForm : System.Windows.Forms.Form
    {
        private sealed class TypeDesc : IComparable
        {
            internal readonly string typeName;
            internal long totalSize;
            internal Color color;
            internal Brush brush;
            internal Pen pen;
            internal bool selected;
            internal Rectangle rect;

            internal TypeDesc(string typeName)
            {
                this.typeName = typeName;
            }

            public int CompareTo(Object o)
            {
                var t = (TypeDesc)o;
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
