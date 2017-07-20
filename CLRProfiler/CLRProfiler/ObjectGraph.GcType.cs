// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace CLRProfiler
{
    public sealed partial class ObjectGraph
    {
        internal sealed class GcType
        {
            internal GcType(string name, int typeID)
            {
                this.name = name;
                this.typeID = typeID;
            }
            internal readonly string name;

            internal int index;
            internal readonly int typeID;

            internal InterestLevel interestLevel;
        }
    }
}
