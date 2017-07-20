// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
namespace CLRProfiler
{
    using System;
    using System.Drawing;

    /// <summary>
    ///    Summary description for Edge.
    /// </summary>
    internal sealed class Edge : IComparable
    {
        internal bool selected;
        internal Brush brush;
        internal Pen pen;
        internal Vertex ToVertex { get; }

        internal Vertex FromVertex { get; }

        internal ulong weight;
        internal int width;
        internal Point fromPoint, toPoint;
        public int CompareTo(Object o)
        {
            var e = (Edge)o;
            int diff = this.ToVertex.rectangle.Top - e.ToVertex.rectangle.Top;
            if (diff != 0)
            {
                return diff;
            }

            diff = this.FromVertex.rectangle.Top - e.FromVertex.rectangle.Top;
            if (diff != 0)
            {
                return diff;
            }

            if (e.weight < this.weight)
            {
                return -1;
            }
            else if (e.weight > this.weight)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }
        internal Edge(Vertex fromVertex, Vertex toVertex)
        {
            this.FromVertex = fromVertex;
            this.ToVertex = toVertex;
            this.weight = 0;
        }

        internal void AddWeight(ulong weight)
        {
            this.weight += weight;
            this.FromVertex.outgoingWeight += weight;
            this.ToVertex.incomingWeight += weight;
        }
    }
}
