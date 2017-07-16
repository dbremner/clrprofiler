using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal class Histogram
    {
        internal int[] typeSizeStacktraceToCount;
        internal readonly ReadNewLog readNewLog;

        internal Histogram(ReadNewLog readNewLog)
        {
            typeSizeStacktraceToCount = new int[10];
            this.readNewLog = readNewLog;
        }

        // Keep track of when this heapdump was taken
        internal readonly int tickIndex;

        internal Histogram(ReadNewLog readNewLog, int tickindex)
        {
            typeSizeStacktraceToCount = new int[10];
            this.readNewLog = readNewLog;
            this.tickIndex = tickindex;
        }
        internal void AddObject(int typeSizeStacktraceIndex, int count)
        {
            while (typeSizeStacktraceIndex >= typeSizeStacktraceToCount.Length)
            {
                typeSizeStacktraceToCount = ReadNewLog.GrowIntVector(typeSizeStacktraceToCount);
            }

            typeSizeStacktraceToCount[typeSizeStacktraceIndex] += count;
        }

        internal bool Empty
        {
            get
            {
                foreach (int count in typeSizeStacktraceToCount)
                {
                    if (count != 0)
                    {
                        return false;
                    }
                }

                return true;
            }
        }

        internal int BuildVertexStack(int stackTraceIndex, Vertex[] funcVertex, ref Vertex[] vertexStack, int skipCount)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
                
            while (vertexStack.Length < stackTrace.Length + 3)
            {
                vertexStack = new Vertex[vertexStack.Length*2];
            }

            for (int i = skipCount; i < stackTrace.Length; i++)
            {
                vertexStack[i-skipCount] = funcVertex[stackTrace[i]];
            }

            return stackTrace.Length - skipCount;
        }

        internal void BuildAllocationTrace(Graph graph, int stackTraceIndex, int typeIndex, ulong size, Vertex[] typeVertex, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 2);

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if ((typeVertex[typeIndex].interestLevel & InterestLevel.Interesting) == InterestLevel.Interesting
                && ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = typeVertex[typeIndex];
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
                fromVertex = toVertex;
                toVertex = graph.BottomVertex;
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(size);
            }
        }

        internal void BuildAssemblyTrace(Graph graph, int stackTraceIndex, Vertex assembly, Vertex typeVertex, Vertex[] funcVertex, ref Vertex[] vertexStack)
        {
            int stackPtr = BuildVertexStack(Math.Abs(stackTraceIndex), funcVertex, ref vertexStack, stackTraceIndex < 0 ? 2 : 0);

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;

            if(typeVertex != null)
            {
                vertexStack[stackPtr++] = typeVertex;
            }
            vertexStack[stackPtr++] = assembly;

            stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
            stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
            for (int i = 0; i < stackPtr; i++)
            {
                fromVertex = toVertex;
                toVertex = vertexStack[i];
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(1);
            }
            fromVertex = toVertex;
            toVertex = graph.BottomVertex;
            edge = graph.FindOrCreateEdge(fromVertex, toVertex);
            edge.AddWeight(1);
        }

        internal void BuildCallTrace(Graph graph, int stackTraceIndex, Vertex[] funcVertex, ref Vertex[] vertexStack, int count, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0);

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight((uint)count);
                }
            }
        }

        internal void BuildHandleAllocationTrace(Graph graph, int stackTraceIndex, uint count, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0);

            Vertex handleVertex = graph.FindOrCreateVertex("Handle", null, null);
            handleVertex.interestLevel = InterestLevel.Interesting;

            Vertex toVertex = graph.TopVertex;
            Vertex fromVertex;
            Edge edge;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = handleVertex;
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(count);
                }
                fromVertex = toVertex;
                toVertex = graph.BottomVertex;
                edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                edge.AddWeight(count);
            }
        }

        internal void BuildTypeVertices(Graph graph, ref Vertex[] typeVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.typeName.Length; i++)
            {
                string typeName = readNewLog.typeName[i];
                if (typeName == null)
                {
                    typeName = string.Format("???? type {0}", i);
                }

                readNewLog.AddTypeVertex(i, typeName, graph, ref typeVertex, filterForm);
            }
        }

        internal int BuildAssemblyVertices(Graph graph, ref Vertex[] typeVertex, FilterForm filterForm)
        {
            int count = 0;
            foreach(string c in readNewLog.assemblies.Keys)
            {
                readNewLog.AddTypeVertex(count++, c, graph, ref typeVertex, filterForm);
            }
            return count;
        }

        internal void BuildFuncVertices(Graph graph, ref Vertex[] funcVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.funcName.Length; i++)
            {
                string name = readNewLog.funcName[i];
                string signature = readNewLog.funcSignature[i];
                if (name == null)
                {
                    name = string.Format("???? function {0}", i);
                }

                if (signature == null)
                {
                    signature = "( ???????? )";
                }

                readNewLog.AddFunctionVertex(i, name, signature, graph, ref funcVertex, filterForm);
            }
        }

        internal Graph BuildAllocationGraph(FilterForm filterForm)
        {
            Vertex[] typeVertex = new Vertex[1];
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.AllocationGraph;

            BuildTypeVertices(graph, ref typeVertex, filterForm);
            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    int typeIndex = stacktrace[0];
                    ulong size = (ulong)stacktrace[1] * (ulong)typeSizeStacktraceToCount[i];

                    BuildAllocationTrace(graph, i, typeIndex, size, typeVertex, funcVertex, ref vertexStack, filterForm);
                }
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }

            graph.BottomVertex.active = false;

            return graph;
        }

        internal Graph BuildAssemblyGraph(FilterForm filterForm)
        {
            Vertex[] assemblyVertex = new Vertex[1];
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] typeVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.AssemblyGraph;

            int count = BuildAssemblyVertices(graph, ref assemblyVertex, filterForm);
            BuildTypeVertices(graph, ref typeVertex, filterForm);
            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for(int i = 0; i < count; i++)
            {
                Vertex v = (Vertex)assemblyVertex[i], tv = null;

                string c = v.name;
                int stackid = readNewLog.assemblies[c];
                if(stackid < 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(-stackid);
                    tv = typeVertex[stacktrace[0]];
                }
                BuildAssemblyTrace(graph, stackid, v, tv, funcVertex, ref vertexStack);
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }
            graph.BottomVertex.active = false;
            return graph;
        }

        internal Graph BuildCallGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.CallGraph;

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    BuildCallTrace(graph, i, funcVertex, ref vertexStack, typeSizeStacktraceToCount[i], filterForm);
                }
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }

            graph.BottomVertex.active = false;

            return graph;
        }

        internal void CalculateCallCounts(uint[] callCount)
        {
            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    callCount[stacktrace[stacktrace.Length-1]] += (uint)typeSizeStacktraceToCount[i];
                }
            }
        }

        internal Graph BuildHandleAllocationGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            Graph graph = new Graph(this);
            graph.graphType = Graph.GraphType.HandleAllocationGraph;

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            for (int i = 0; i < typeSizeStacktraceToCount.Length; i++)
            {
                if (typeSizeStacktraceToCount[i] > 0)
                {
                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(i);

                    uint count = (uint)typeSizeStacktraceToCount[i];

                    BuildHandleAllocationTrace(graph, i, count, funcVertex, ref vertexStack, filterForm);
                }
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }

            graph.BottomVertex.active = false;

            return graph;
        }

    }
}