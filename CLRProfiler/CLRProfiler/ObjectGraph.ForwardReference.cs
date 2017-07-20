// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace CLRProfiler
{
    public partial class ObjectGraph
    {
        private class ForwardReference
        {
            internal ForwardReference(GcObject source, int referenceNumber, ForwardReference next)
            {
                this.source = source;
                this.referenceNumber = referenceNumber;
                this.next = next;
            }
            internal readonly GcObject         source;            // the object having the forward reference
            internal readonly int              referenceNumber;   // the number of the reference within the object
            internal readonly ForwardReference next;              // next forward reference to the same address
        }
    }
}
