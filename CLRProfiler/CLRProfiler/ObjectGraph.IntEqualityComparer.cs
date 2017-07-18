// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;

namespace CLRProfiler
{
    public partial class ObjectGraph
    {
        internal class IntEqualityComparer : IEqualityComparer<int>
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
