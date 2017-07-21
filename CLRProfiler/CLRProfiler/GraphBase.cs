using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Drawing;
using JetBrains.Annotations;

namespace CLRProfiler
{
	/// <summary>
	/// Summary description for GraphBase.
	/// </summary>
	public sealed class GraphBase
	{
		#region private data member
	    private ulong totalWeight;
		private readonly int totalHeight = 100;
		private float scale = 1.0f;
	    //private Graph callGraph = null;
		#endregion
		#region public data member
		public List<ArrayList> levelList;
		#endregion
		public GraphBase()
		{
		
		}
		#region public methods
		internal void GetAllocationGraph([NotNull] ReadLogResult readLogResult)
		{
			basegraph = readLogResult.allocatedHistogram.BuildAllocationGraph(new FilterForm());
			PlaceVertices();
		}

        /*public void GetCallGraph(ReadLogResult readLogResult)
		{
			callGraph = readLogResult.callstackHistogram.BuildCallGraph();
			PlaceVertices();
		}

		public void GetFunctionGraph(ReadLogResult readLogResult)
		{
			graph = readLogResult.functionList.BuildFunctionGraph();
			PlaceVertices();
		}*/

	    public int SelectedVertexCount() => basegraph.SelectedVertexCount();
	    #endregion
		#region private methods
	    [NotNull]
	    private List<ArrayList> BuildLevels([NotNull] Graph g)
		{
			var al = new List<ArrayList>();
			for (int level = 0; level <= g.BottomVertex.level; level++)
			{
				al.Add(new ArrayList());
			}
			foreach (Vertex v in g.vertices.Values)
			{
				if (v.level <= g.BottomVertex.level)
				{
					var all = al[v.level];
					all.Add(v);
				}
				else
				{
					Debug.Assert(v.level == int.MaxValue);
				}
			}
			foreach (var all in al)
			{
				all.Sort();
			}
			return al;
		}

        internal string formatWeight(ulong weight)
		{
			if (basegraph.graphType == Graph.GraphType.CallGraph)
			{
				if (weight == 1)
                {
                    return "1 call";
                }
                else
                {
                    return string.Format("{0} calls", weight);
                }
            }
			if(basegraph.graphType == Graph.GraphType.AssemblyGraph)
			{
				if(weight == 1)
				{
					return "1 assembly";
				}
				else
				{
					return weight + " assemblies";
				}
			}
			else
			{
				double w = weight;
				string byteString = "bytes";
				if (w >= 1024)
				{
					w /= 1024;
					byteString = "kB   ";
				}
				if (w >= 1024)
				{
					w /= 1024;
					byteString = "MB   ";
				}
				if (w >= 1024)
				{
					w /= 1024;
					byteString = "GB   ";
				}
				string format = "{0,4:f0} {1} ({2:f2}%)";
				if (w < 10)
                {
                    format = "{0,4:f1} {1} ({2:f2}%)";
                }

                return string.Format(format, w, byteString, weight*100.0/totalWeight);
			}
		}

		private void PlaceEdges(float scale)
		{
			foreach (Vertex v in basegraph.vertices.Values)
			{
				PlaceEdges(v.incomingEdges.Values, true, scale);
				PlaceEdges(v.outgoingEdges.Values, false, scale);
			}
		}
		private void PlaceEdges([NotNull] ICollection edgeCollection, bool isIncoming, float scale)
		{
			var edgeList = new ArrayList(edgeCollection);
			edgeList.Sort();
			foreach (Edge e in edgeList)
			{
				float fwidth = e.weight*scale;
			}
		}

		private void PlaceVertices()
	    {
	        basegraph.AssignLevelsToVertices();
	        totalWeight = 0;
	        foreach (Vertex v in basegraph.vertices.Values)
	        {
	            v.weight = v.incomingWeight;
	            if (v.weight < v.outgoingWeight)
	            {
	                v.weight = v.outgoingWeight;
	            }

	            if (basegraph.graphType == Graph.GraphType.CallGraph)
	            {
	                if (totalWeight < v.weight)
	                {
	                    totalWeight = v.weight;
	                }
	            }
	        }
	        if (basegraph.graphType != Graph.GraphType.CallGraph)
	        {
	            totalWeight = basegraph.TopVertex.weight;
	        }

	        if (totalWeight == 0)
	        {
	            totalWeight = 1;
	        }

	        var al = BuildLevels(basegraph);
	        levelList = al;
	        scale = (float) totalHeight / totalWeight;
	        for (int level = basegraph.TopVertex.level;
	            level <= basegraph.BottomVertex.level;
	            level++)
	        {
	            var all = al[level];
	            foreach (Vertex v in all)
	            {
	                if (basegraph.graphType == Graph.GraphType.CallGraph)
	                {
	                    v.basicWeight = v.incomingWeight - v.outgoingWeight;
	                    if (v.basicWeight < 0)
	                    {
	                        v.basicWeight = 0;
	                    }

	                    v.weightString = string.Format("Gets {0}, causes {1}",
	                        formatWeight(v.basicWeight),
	                        formatWeight(v.outgoingWeight));
	                }
	                else
	                {
	                    if (v.count == 0)
	                    {
	                        v.weightString = formatWeight(v.weight);
	                    }
	                    else if (v.count == 1)
	                    {
	                        v.weightString = string.Format("{0}  (1 object, {1})", formatWeight(v.weight),
	                            formatWeight(v.basicWeight));
	                    }
	                    else
	                    {
	                        v.weightString = string.Format("{0}  ({1} objects, {2})", formatWeight(v.weight), v.count,
	                            formatWeight(v.basicWeight));
	                    }
	                }
	            }
	            int y = 10;
	            ulong levelWeight = 0;
	            foreach (Vertex v in all)
	            {
	                levelWeight += v.weight;
	            }

	            float levelHeight = levelWeight * scale;
	            if (levelHeight < totalHeight * 0.5)
	            {
	                y += (int) ((totalHeight - levelHeight) * 2);
	            }

	            foreach (Vertex v in all)
	            {
	                // For the in-between vertices, sometimes it's good
	                // to shift them down a little to line them up with
	                // whatever is going into them. Unless of course
	                // we would need to shift too much...
	                if (v.level < basegraph.BottomVertex.level - 1)
	                {
	                    ulong highestWeight = 0;
	                    foreach (Edge e in v.incomingEdges.Values)
	                    {
	                        if (e.weight > highestWeight && e.FromVertex.level < level)
	                        {
	                            highestWeight = e.weight;
	                        }
	                    }
	                }
	                float fHeight = v.weight * scale;
	                int iHeight = (int) fHeight;
	                if (iHeight < 1)
	                {
	                    iHeight = 1;
	                }
	                PlaceEdges(v.outgoingEdges.Values, false, scale);
	            }
	        }
	        PlaceEdges(scale);
	    }
	    #endregion
		internal Graph basegraph { get; private set; }
	}
}
