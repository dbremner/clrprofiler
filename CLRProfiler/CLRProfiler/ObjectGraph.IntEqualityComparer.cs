// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;

namespace CLRProfiler
{
    public sealed partial class ObjectGraph
    {
        internal sealed class IntEqualityComparer : IEqualityComparer<int>
        {
            public bool Equals(int a, int b)
            {
                return a == b;
            }
            public int GetHashCode(int a)
            {
                return a;
            }
        }
    }
}
