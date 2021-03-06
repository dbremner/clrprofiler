﻿// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal sealed partial class GraphViewForm : System.Windows.Forms.Form
    {
        private sealed class CompareVerticesByScore : IComparer
        {
            [NotNull] private readonly Dictionary<Vertex, double> scoreOfVertex;

            internal CompareVerticesByScore([NotNull] Dictionary<Vertex, double> scoreOfVertex)
            {
                this.scoreOfVertex = scoreOfVertex;
            }

            int IComparer.Compare(object x, object y)
            {
                double scoreX = scoreOfVertex[(Vertex)x];
                double scoreY = scoreOfVertex[(Vertex)y];
                if (scoreX < scoreY)
                {
                    return 1;
                }
                else if (scoreX > scoreY)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
        }
    }
}
