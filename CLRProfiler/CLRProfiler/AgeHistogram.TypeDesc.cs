﻿// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Drawing;

namespace CLRProfiler
{
    public sealed partial class AgeHistogram : System.Windows.Forms.Form
    {
        private sealed class TypeDesc : IComparable, IComparable<TypeDesc>
        {
            internal readonly string typeName;
            internal ulong totalSize;
            internal Color color;
            internal Brush brush;
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

            public int CompareTo(TypeDesc other)
            {
                var t = (TypeDesc)other;
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

