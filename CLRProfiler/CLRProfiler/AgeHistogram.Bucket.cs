// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;

namespace CLRProfiler
{
    public partial class AgeHistogram : System.Windows.Forms.Form
    {
        struct Bucket
        {
            internal ulong totalSize;
            internal Dictionary<TypeDesc, SizeCount> typeDescToSizeCount;
            internal bool selected;
            internal double minAge;
            internal double maxAge;
            internal string minComment;
            internal string maxComment;
        }
    }
}

