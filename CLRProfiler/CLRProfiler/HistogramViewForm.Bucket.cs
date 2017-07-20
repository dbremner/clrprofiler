// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;

namespace CLRProfiler
{
    public partial class HistogramViewForm : System.Windows.Forms.Form
    {
        private struct Bucket
        {
            internal int minSize;
            internal int maxSize;
            internal ulong totalSize;
            internal Dictionary<TypeDesc, SizeCount> typeDescToSizeCount;
            internal bool selected;
        }
    }
}
