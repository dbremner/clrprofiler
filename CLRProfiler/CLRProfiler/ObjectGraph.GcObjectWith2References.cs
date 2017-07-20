// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;
using System.Diagnostics;

namespace CLRProfiler
{
    public partial class ObjectGraph
    {
        private class GcObjectWith2References : GcObjectWith1Reference
        {
            protected GcObject reference1;
            internal override IEnumerable<GcObject> References
            {
                get
                {
                    yield return reference0;
                    yield return reference1;
                }
            }

            internal override void SetReference(int referenceNumber, GcObject target)
            {
                switch (referenceNumber)
                {
                    case 0: reference0 = target; break;
                    case 1: reference1 = target; break;
                    default: Debug.Assert(referenceNumber == 0); break;
                }
            }
        }
    }
}
