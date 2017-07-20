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
        private class GcObjectWithManyReferences : GcObject
        {
            private readonly GcObject[] references;
            internal GcObjectWithManyReferences(int numberOfReferences)
            {
                references = new GcObject[numberOfReferences];
            }

            internal override IEnumerable<GcObject> References
            {
                get
                {
                    for (int i = 0; i < references.Length; i++)
                    {
                        yield return references[i];
                    }
                }
            }

            internal override void SetReference(int referenceNumber, GcObject target)
            {
                references[referenceNumber] = target;
            }
        }
    }
}
