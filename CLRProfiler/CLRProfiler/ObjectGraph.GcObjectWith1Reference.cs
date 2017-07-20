// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;
using System.Diagnostics;

namespace CLRProfiler
{
    public sealed partial class ObjectGraph
    {
        private class GcObjectWith1Reference : GcObject
        {
            protected GcObject reference0;
            internal override IEnumerable<GcObject> References
            {
                get
                {
                    yield return reference0;
                }
            }
            internal override void SetReference(int referenceNumber, GcObject target)
            {
                Debug.Assert(referenceNumber == 0);
                reference0 = target;
            }
        }
    }
}
