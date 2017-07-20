// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

namespace CLRProfiler
{
    public sealed partial class AgeHistogram : System.Windows.Forms.Form
    {
        private sealed class SizeCount
        {
            internal ulong size;
            internal uint count;

            public SizeCount()
            {
            }
        }
    }
}

