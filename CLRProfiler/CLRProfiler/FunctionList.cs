using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal partial class FunctionList
    {
        readonly ReadNewLog readNewLog;
        readonly List<FunctionDescriptor> functionList;

        internal FunctionList(ReadNewLog readNewLog)
        {
            this.readNewLog = readNewLog;
            this.functionList = new List<FunctionDescriptor>();
        }

        internal void Add(int functionId, int funcCallStack, uint funcSize, int funcModule)
        {
            functionList.Add(new FunctionDescriptor(functionId, funcCallStack, funcSize, funcModule));
        }

        internal bool Empty => functionList.Count == 0;

        void BuildFuncVertices(Graph graph, ref Vertex[] funcVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.funcName.Length; i++)
            {
                string name = readNewLog.funcName[i];
                string signature = readNewLog.funcSignature[i];
                if (name != null && signature != null)
                {
                    readNewLog.AddFunctionVertex(i, name, signature, graph, ref funcVertex, filterForm);
                }
            }
        }

        int BuildVertexStack(int stackTraceIndex, Vertex[] funcVertex, ref Vertex[] vertexStack, int skipCount)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
                
            while (vertexStack.Length < stackTrace.Length+1)
            {
                vertexStack = new Vertex[vertexStack.Length*2];
            }

            for (int i = skipCount; i < stackTrace.Length; i++)
            {
                vertexStack[i-skipCount] = funcVertex[stackTrace[i]];
            }

            return stackTrace.Length - skipCount;
        }

        void BuildFunctionTrace(Graph graph, int stackTraceIndex, int funcIndex, ulong size, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0);

            Vertex toVertex = graph.TopVertex;
            if ((funcVertex[funcIndex].interestLevel & InterestLevel.Interesting) == InterestLevel.Interesting
                && ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = funcVertex[funcIndex];
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                Edge edge;
                Vertex fromVertex;
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

        internal Graph BuildFunctionGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            var graph = new Graph(this, Graph.GraphType.FunctionGraph);

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            foreach (var fd in functionList)
            {
                BuildFunctionTrace(graph, fd.funcCallStack, fd.functionId, fd.funcSize, funcVertex, ref vertexStack, filterForm);
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }

            graph.BottomVertex.active = false;

            return graph;
        }

        void BuildModVertices(Graph graph, ref Vertex[] modVertex, FilterForm filterForm)
        {
            for (int i = 0; i < readNewLog.modBasicName.Length; i++)
            {
                string basicName = readNewLog.modBasicName[i];
                string fullName = readNewLog.modFullName[i];
                if (basicName != null && fullName != null)
                {
                    readNewLog.AddFunctionVertex(i, basicName, fullName, graph, ref modVertex, filterForm);
                    modVertex[i].basicName = basicName;
                    modVertex[i].basicSignature = fullName;
                }
            }
        }

        int FunctionsInSameModule(int modIndex, int stackTraceIndex)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
            int result = 0;
            for (int i = stackTrace.Length - 1; i >= 0; i--)
            {
                int funcIndex = stackTrace[i];
                if (readNewLog.funcModule[funcIndex] == modIndex)
                {
                    result++;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        void BuildModuleTrace(Graph graph, int stackTraceIndex, int modIndex, ulong size, Vertex[] funcVertex, Vertex[] modVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            int functionsToSkip = FunctionsInSameModule(modIndex, stackTraceIndex);
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0) - functionsToSkip;

            Vertex toVertex = graph.TopVertex;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = modVertex[modIndex];
                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                Edge edge;
                Vertex fromVertex;
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

        internal Graph BuildModuleGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];
            Vertex[] modVertex = new Vertex[1];

            var graph = new Graph(this, Graph.GraphType.ModuleGraph);

            BuildFuncVertices(graph, ref funcVertex, filterForm);
            BuildModVertices(graph, ref modVertex, filterForm);

            foreach (var fd in functionList)
            {
                BuildModuleTrace(graph, fd.funcCallStack, fd.funcModule, fd.funcSize, funcVertex, modVertex, ref vertexStack, filterForm);
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }

            graph.BottomVertex.active = false;

            return graph;
        }

        string ClassNameOfFunc(int funcIndex)
        {
            string funcName = readNewLog.funcName[funcIndex];
            int colonColonIndex = funcName.IndexOf("::");
            if (colonColonIndex > 0)
            {
                return funcName.Substring(0, colonColonIndex);
            }
            else
            {
                return funcName;
            }
        }

        int FunctionsInSameClass(string className, int stackTraceIndex)
        {
            int[] stackTrace = readNewLog.stacktraceTable.IndexToStacktrace(stackTraceIndex);
            int result = 0;
            for (int i = stackTrace.Length - 1; i >= 0; i--)
            {
                int funcIndex = stackTrace[i];
                if (ClassNameOfFunc(funcIndex) == className)
                {
                    result++;
                }
                else
                {
                    break;
                }
            }
            return result;
        }

        void BuildClassTrace(Graph graph, int stackTraceIndex, int funcIndex, ulong size, Vertex[] funcVertex, ref Vertex[] vertexStack, FilterForm filterForm)
        {
            string className = ClassNameOfFunc(funcIndex);
            int functionsToSkip = FunctionsInSameClass(className, stackTraceIndex);
            int stackPtr = BuildVertexStack(stackTraceIndex, funcVertex, ref vertexStack, 0) - functionsToSkip;

            Vertex toVertex = graph.TopVertex;
            if (ReadNewLog.InterestingCallStack(vertexStack, stackPtr, filterForm))
            {
                vertexStack[stackPtr] = graph.FindOrCreateVertex(className, null, null);
                vertexStack[stackPtr].interestLevel = filterForm.IsInterestingMethodName(className, null)
                    ? InterestLevel.Interesting | filterForm.InterestLevelForParentsAndChildren() : InterestLevel.Ignore;

                stackPtr++;
                stackPtr = ReadNewLog.FilterVertices(vertexStack, stackPtr);
                stackPtr = Vertex.SqueezeOutRepetitions(vertexStack, stackPtr);
                Edge edge;
                Vertex fromVertex;
                for (int i = 0; i < stackPtr; i++)
                {
                    fromVertex = toVertex;
                    toVertex = vertexStack[i];
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
                if (toVertex != graph.TopVertex)
                {
                    fromVertex = toVertex;
                    toVertex = graph.BottomVertex;
                    edge = graph.FindOrCreateEdge(fromVertex, toVertex);
                    edge.AddWeight(size);
                }
            }
        }

        internal Graph BuildClassGraph(FilterForm filterForm)
        {
            Vertex[] funcVertex = new Vertex[1];
            Vertex[] vertexStack = new Vertex[1];

            var graph = new Graph(this, Graph.GraphType.ClassGraph);

            BuildFuncVertices(graph, ref funcVertex, filterForm);

            foreach (var fd in functionList)
            {
                BuildClassTrace(graph, fd.funcCallStack, fd.functionId, fd.funcSize, funcVertex, ref vertexStack, filterForm);
            }

            foreach (Vertex v in graph.vertices.Values)
            {
                v.active = true;
            }

            graph.BottomVertex.active = false;

            return graph;
        }

        internal void ReportCallCountSizes(Histogram callstackHistogram)
        {
            uint[] callCount = new uint[readNewLog.funcName.Length];
            callstackHistogram.CalculateCallCounts(callCount);
            Console.WriteLine("{0},{1},{2} {3}", "# Calls", "Function Size", "Function Name", "Function Signature");
            foreach (var fd in functionList)
            {
                Console.WriteLine("{0},{1},{2} {3}", callCount[fd.functionId], fd.funcSize, readNewLog.funcName[fd.functionId], readNewLog.funcSignature[fd.functionId]);
            }
        }
    }
}