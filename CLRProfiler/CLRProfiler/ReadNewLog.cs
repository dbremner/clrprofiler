/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  Histogram, ReadNewLog, SampleObjectTable, LiveObjectTable, etc.
 *
 * Description: Log file parser and various graph generators
 */

using System;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Windows.Forms;

namespace CLRProfiler
{
    internal partial class ReadNewLog
    {
        internal ReadNewLog(string fileName)
        {
            //
            // TODO: Add constructor logic here
            //
            assemblies = new Dictionary<string/*assembly name*/, int/*stack id*/>();
            assembliesJustLoaded = new Dictionary<int/*thread id*/, List<string>>();
            typeName = new string[1000];
            funcName = new string[1000];
            funcSignature = new string[1000];
            funcModule = new int[1000];
            modBasicName = new string[10];
            modFullName = new string[10];
            commentEventList = new EventList();
            gcEventList = new EventList();
            heapDumpEventList = new EventList();
            finalizableTypes = new Dictionary<int/*type id*/,bool>();
            gcCount = new int[4];
            inducedGcCount = new int[3];
            generationSize = new ulong[4];
            cumulativeGenerationSize = new ulong[4];

            this.fileName = fileName;

            this.typeSignatureIdHash = new Dictionary<string/*type name*/, int/*type id*/>();
            this.funcSignatureIdHash = new Dictionary<string/*func name*/, int/*func id*/>();

            this.progressFormVisible = true;
        }

        internal ReadNewLog(string fileName, bool progressFormVisible) : this(fileName)
        {
            this.progressFormVisible = progressFormVisible;
        }

        internal StacktraceTable stacktraceTable;
        internal string fileName;

        private StreamReader r;
        private byte[] buffer;
        private int bufPos;
        private int bufLevel;
        private int c;
        private int line;
        internal long pos;
        private long lastLineStartPos;
        internal readonly Dictionary<int/*thread id*/, List<string>> assembliesJustLoaded;
        internal readonly Dictionary<string/*assembly name*/, int/*stack id*/> assemblies;
        internal string[] typeName;
        internal string[] funcName;
        internal string[] funcSignature;
        internal int[] funcModule;
        internal string[] modBasicName;
        internal string[] modFullName;
        internal readonly Dictionary<int/*type id*/, bool> finalizableTypes;
        internal readonly Dictionary<string/*type name*/, int/*type id*/> typeSignatureIdHash;
        internal readonly Dictionary<string/*func name*/, int/*func id*/> funcSignatureIdHash;
        internal int maxTickIndex;
        private readonly bool progressFormVisible;
        internal readonly int[] inducedGcCount;
        internal readonly int[] gcCount;
        internal readonly ulong[] generationSize;
        internal readonly ulong[] cumulativeGenerationSize;
        internal readonly EventList commentEventList;
        internal readonly EventList gcEventList;
        internal readonly EventList heapDumpEventList;

        private static void EnsureVertexCapacity(int id, ref Vertex[] vertexArray)
        {
            Debug.Assert(id >= 0);
            if (id < vertexArray.Length)
            {
                return;
            }

            int newLength = vertexArray.Length*2;
            if (newLength <= id)
            {
                newLength = id + 1;
            }

            Vertex[] newVertexArray = new Vertex[newLength];
            Array.Copy(vertexArray, 0, newVertexArray, 0, vertexArray.Length);
            vertexArray = newVertexArray;
        }

        private static void EnsureStringCapacity(int id, ref string[] stringArray)
        {
            Debug.Assert(id >= 0);
            if (id < stringArray.Length)
            {
                return;
            }

            int newLength = stringArray.Length*2;
            if (newLength <= id)
            {
                newLength = id + 1;
            }

            string[] newStringArray = new string[newLength];
            Array.Copy(stringArray, 0, newStringArray, 0, stringArray.Length);
            stringArray = newStringArray;
        }

        private static void EnsureIntCapacity(int id, ref int[] intArray)
        {
            Debug.Assert(id >= 0);
            if (id < intArray.Length)
            {
                return;
            }

            int newLength = intArray.Length*2;
            if (newLength <= id)
            {
                newLength = id + 1;
            }

            int[] newIntArray = new int[newLength];
            Array.Copy(intArray, 0, newIntArray, 0, intArray.Length);
            intArray = newIntArray;
        }

        internal void AddTypeVertex(int typeId, string typeName, Graph graph, ref Vertex[] typeVertex, FilterForm filterForm)
        {
            EnsureVertexCapacity(typeId, ref typeVertex);
            typeVertex[typeId] = graph.FindOrCreateVertex(typeName, null, null);
            typeVertex[typeId].interestLevel = filterForm.IsInterestingTypeName(typeName, null, finalizableTypes.ContainsKey(typeId))
                ? InterestLevel.Interesting | filterForm.InterestLevelForParentsAndChildren() : InterestLevel.Ignore;
        }

        internal void AddFunctionVertex(int funcId, string functionName, string signature, Graph graph, ref Vertex[] funcVertex, FilterForm filterForm)
        {
            EnsureVertexCapacity(funcId, ref funcVertex);
            int moduleId = funcModule[funcId];
            string moduleName = null;
            if (moduleId >= 0)
            {
                moduleName = modBasicName[moduleId];
            }

            funcVertex[funcId] = graph.FindOrCreateVertex(functionName, signature, moduleName);
            funcVertex[funcId].interestLevel = filterForm.IsInterestingMethodName(functionName, signature)
                ? InterestLevel.Interesting | filterForm.InterestLevelForParentsAndChildren() : InterestLevel.Ignore;
        }

        private void AddTypeName(int typeId, string typeName)
        {
            EnsureStringCapacity(typeId, ref this.typeName);
            this.typeName[typeId] = typeName;
            typeSignatureIdHash[typeName] = typeId;
        }

        private int FillBuffer()
        {
            bufPos = 0;
            bufLevel = r.BaseStream.Read(buffer, 0, buffer.Length);
            if (bufPos < bufLevel)
            {
                return buffer[bufPos++];
            }
            else
            {
                return -1;
            }
        }

        internal int ReadChar()
        {
            pos++;
            if (bufPos < bufLevel)
            {
                return buffer[bufPos++];
            }
            else
            {
                return FillBuffer();
            }
        }

        private int ReadHex()
        {
            int value = 0;
            while (true)
            {
                c = ReadChar();
                int digit = c;
                if (digit >= '0' && digit <= '9')
                {
                    digit -= '0';
                }
                else if (digit >= 'a' && digit <= 'f')
                {
                    digit -= 'a' - 10;
                }
                else if (digit >= 'A' && digit <= 'F')
                {
                    digit -= 'A' - 10;
                }
                else
                {
                    return value;
                }

                value = value * 16 + digit;
            }
        }

        private int ReadInt()
        {
            while (c == ' ' || c == '\t')
            {
                c = ReadChar();
            }

            bool negative = false;
            if (c == '-')
            {
                negative = true;
                c = ReadChar();
            }
            if (c >= '0' && c <= '9')
            {
                int value = 0;
                if (c == '0')
                {
                    c = ReadChar();
                    if (c == 'x' || c == 'X')
                    {
                        value = ReadHex();
                    }
                }
                while (c >= '0' && c <= '9')
                {
                    value = value * 10 + c - '0';
                    c = ReadChar();
                }

                if (negative)
                {
                    value = -value;
                }

                return value;
            }
            else
            {
                return -1;
            }
        }

        private uint ReadUInt()
        {
            return (uint)ReadInt();
        }

        private long ReadLongHex()
        {
            long value = 0;
            while (true)
            {
                c = ReadChar();
                int digit = c;
                if (digit >= '0' && digit <= '9')
                {
                    digit -= '0';
                }
                else if (digit >= 'a' && digit <= 'f')
                {
                    digit -= 'a' - 10;
                }
                else if (digit >= 'A' && digit <= 'F')
                {
                    digit -= 'A' - 10;
                }
                else
                {
                    return value;
                }

                value = value * 16 + digit;
            }
        }

        private long ReadLong()
        {
            while (c == ' ' || c == '\t')
            {
                c = ReadChar();
            }

            bool negative = false;
            if (c == '-')
            {
                negative = true;
                c = ReadChar();
            }
            if (c >= '0' && c <= '9')
            {
                long value = 0;
                if (c == '0')
                {
                    c = ReadChar();
                    if (c == 'x' || c == 'X')
                    {
                        value = ReadLongHex();
                    }
                }
                while (c >= '0' && c <= '9')
                {
                    value = value * 10 + c - '0';
                    c = ReadChar();
                }

                if (negative)
                {
                    value = -value;
                }

                return value;
            }
            else
            {
                return -1;
            }
        }

        private ulong ReadULong()
        {
            return (ulong)ReadLong();
        }

        private string ReadString(StringBuilder sb, char delimiter, bool stopAfterRightParen, int maxLength)
        {
            // Name may contain spaces if they are in angle brackets.
            // Example: <Module>::std_less<unsigned void>.()
            // The name may be truncated at 255 chars by profilerOBJ.dll
            sb.Length = 0;
            int angleBracketsScope = 0;
            while (c > delimiter || angleBracketsScope != 0 && sb.Length < maxLength)
            {
                if (c == '\\')
                {
                    c = ReadChar();
                    if (c == 'u')
                    {
                        // handle unicode escape
                        c = ReadChar();
                        int count = 0;
                        int hexVal = 0;
                        while (count < 4 && ('0' <= c && c <= '9' || 'a' <= c && c <= 'f'))
                        {
                            count++;
                            hexVal = 16 * hexVal + ((c <= '9') ? (c - '0') : (c - 'a' + 10));
                            c = ReadChar();
                        }
                        sb.Append((char)hexVal);
                    }
                    else
                    {
                        // handle other escaped character - append it without inspecting it,
                        // it doesn't count as a delimiter or anything else
                        sb.Append((char)c);
                        c = ReadChar();
                    }
                    continue;
                }

                // non-escaped character - always append it
                sb.Append((char)c);

                if (c == '<')
                {
                    angleBracketsScope++;
                }
                else if (c == '>' && angleBracketsScope > 0)
                {
                    angleBracketsScope--;
                }
                else if (stopAfterRightParen && c == ')')
                {
                    // we have already appened it above - now read the character after it.
                    c = ReadChar();
                    break;
                }

                c = ReadChar();
            }
            return sb.ToString();
        }

        private int ForcePosInt()
        {
            int value = ReadInt();
            if (value >= 0)
            {
                return value;
            }
            else
            {
                throw new Exception(string.Format("Bad format in log file {0} line {1}", fileName, line));
            }
        }

        internal static int[] GrowIntVector(int[] vector)
        {
            int[] newVector = new int[vector.Length*2];
            for (int i = 0; i < vector.Length; i++)
            {
                newVector[i] = vector[i];
            }

            return newVector;
        }

        internal static ulong[] GrowULongVector(ulong[] vector)
        {
            ulong[] newVector = new ulong[vector.Length*2];
            for (int i = 0; i < vector.Length; i++)
            {
                newVector[i] = vector[i];
            }

            return newVector;
        }

        internal static bool InterestingCallStack(Vertex[] vertexStack, int stackPtr, FilterForm filterForm)
        {
            if (stackPtr == 0)
            {
                return filterForm.methodFilters.Length == 0;
            }

            if ((vertexStack[stackPtr-1].interestLevel & InterestLevel.Interesting) == InterestLevel.Interesting)
            {
                return true;
            }

            for (int i = stackPtr-2; i >= 0; i--)
            {
                switch (vertexStack[i].interestLevel & InterestLevel.InterestingChildren)
                {
                    case    InterestLevel.Ignore:
                        break;

                    case    InterestLevel.InterestingChildren:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        internal static int FilterVertices(Vertex[] vertexStack, int stackPtr)
        {
            bool display = false;
            for (int i = 0; i < stackPtr; i++)
            {
                Vertex vertex = vertexStack[i];
                switch (vertex.interestLevel & InterestLevel.InterestingChildren)
                {
                    case    InterestLevel.Ignore:
                        if (display)
                        {
                            vertex.interestLevel |= InterestLevel.Display;
                        }

                        break;

                    case    InterestLevel.InterestingChildren:
                        display = true;
                        break;

                    default:
                        display = false;
                        break;
                }
            }
            display = false;
            for (int i = stackPtr-1; i >= 0; i--)
            {
                Vertex vertex = vertexStack[i];
                switch (vertex.interestLevel & InterestLevel.InterestingParents)
                {
                    case    InterestLevel.Ignore:
                        if (display)
                        {
                            vertex.interestLevel |= InterestLevel.Display;
                        }

                        break;

                    case    InterestLevel.InterestingParents:
                        display = true;
                        break;

                    default:
                        display = false;
                        break;
                }
            }
            int newStackPtr = 0;
            for (int i = 0; i < stackPtr; i++)
            {
                Vertex vertex = vertexStack[i];
                if ((vertex.interestLevel & (InterestLevel.Display|InterestLevel.Interesting)) != InterestLevel.Ignore)
                {
                    vertexStack[newStackPtr++] = vertex;
                    vertex.interestLevel &= ~InterestLevel.Display;
                }
            }
            return newStackPtr;
        }

        private TimePos[] timePos;
        private int timePosCount, timePosIndex;
        private const int maxTimePosCount = (1<<23)-1; // ~8,000,000 entries

        private void GrowTimePos()
        {
            TimePos[] newTimePos = new TimePos[2*timePos.Length];
            for (int i = 0; i < timePos.Length; i++)
            {
                newTimePos[i] = timePos[i];
            }

            timePos = newTimePos;
        }

        private int AddTimePos(int tick, long pos)
        {
            double time = tick*0.001;
            
            // The time stamps can not always be taken at face value.
            // The two problems we try to fix here are:
            // - the time may wrap around (after about 50 days).
            // - on some MP machines, different cpus could drift apart
            // We solve the first problem by adding 2**32*0.001 if the
            // time appears to jump backwards by more than 2**31*0.001.
            // We "solve" the second problem by ignoring time stamps
            // that still jump backward in time.
            double lastTime = 0.0;
            if (timePosIndex > 0)
            {
                lastTime = timePos[timePosIndex-1].time;
            }
            // correct possible wraparound
            while (time + (1L<<31)*0.001 < lastTime)
            {
                time += (1L<<32)*0.001;
            }

            // ignore times that jump backwards
            if (time < lastTime)
            {
                return timePosIndex - 1;
            }

            while (timePosCount >= timePos.Length)
            {
                GrowTimePos();
            }

            // we have only 23 bits to encode allocation time.
            // to avoid running out for long running measurements, we decrease time resolution
            // as we chew up slots. below algorithm uses 1 millisecond resolution for the first
            // million slots, 2 milliseconds for the second million etc. this gives about
            // 2 million seconds time range or 23 days. This is if we really have a time stamp
            // every millisecond - if not, the range is much larger...
            double minimumTimeInc = 0.000999*(1<<timePosIndex/(maxTimePosCount/8));
            if (timePosCount < maxTimePosCount && (time - lastTime >= minimumTimeInc))
            {
                if (timePosIndex < timePosCount)
                {
                    // This is the case where we read the file again for whatever reason
                    Debug.Assert(timePos[timePosIndex].time == time && timePos[timePosIndex].pos == pos);
                    return timePosIndex++;
                }
                else
                {
                    timePos[timePosCount] = new TimePos(time, pos);
                    timePosIndex++;
                    return timePosCount++;
                }
            }
            else
            {
                return timePosIndex - 1;
            }
        }

        // variant of above to give comments their own tick index
        private int AddTimePos(long pos)
        {
            double lastTime = 0.0;
            if (timePosIndex > 0)
            {
                lastTime = timePos[timePosIndex-1].time;
            }

            while (timePosCount >= timePos.Length)
            {
                GrowTimePos();
            }

            // stop giving comments their own tick index if we have already
            // burned half the available slots
            if (timePosCount < maxTimePosCount/2)
            {
                if (timePosIndex < timePosCount)
                {
                    // This is the case where we read the file again for whatever reason
                    Debug.Assert(timePos[timePosIndex].time == lastTime && timePos[timePosIndex].pos == pos);
                    return timePosIndex++;
                }
                else
                {
                    timePos[timePosCount] = new TimePos(lastTime, pos);
                    timePosIndex++;
                    return timePosCount++;
                }
            }
            else
            {
                return timePosIndex - 1;
            }
        }

        internal double TickIndexToTime(int tickIndex)
        {
            return timePos[tickIndex].time;
        }

        internal long TickIndexToPos(int tickIndex)
        {
            return timePos[tickIndex].pos;
        }

        internal int TimeToTickIndex(double time)
        {
            int l = 0;
            int r = timePosCount-1;
            if (time < timePos[l].time)
            {
                return l;
            }

            if (timePos[r].time <= time)
            {
                return r;
            }

            // binary search - loop invariant is timePos[l].time <= time && time < timePos[r].time
            // loop terminates because loop condition implies l < m < r and so the interval
            // shrinks on each iteration
            while (l + 1 < r)
            {
                int m = (l + r) / 2;
                if (time < timePos[m].time)
                {
                    r = m;
                }
                else
                {
                    l = m;
                }
            }

            // we still have the loop invariant timePos[l].time <= time && time < timePos[r].time
            // now we just return the index that gives the closer match.
            if (time - timePos[l].time < timePos[r].time - time)
            {
                return l;
            }
            else
            {
                return r;
            }
        }

        private enum GcRootKind
        {
            Other = 0x0,
            Stack = 0x1,
            Finalizer = 0x2,
            Handle = 0x3,
        }

        [Flags]
        private enum GcRootFlags
        {
            Pinning = 0x1,
            WeakRef = 0x2,
            Interior = 0x4,
            Refcounted = 0x8,
        }

        internal void ReadFile(long startFileOffset, long endFileOffset, ReadLogResult readLogResult)
        {
            ReadFile(startFileOffset, endFileOffset, readLogResult, -1);
        }

        internal void ReadFile(long startFileOffset, long endFileOffset, ReadLogResult readLogResult, int requestedIndex)
        {
            var progressForm = new ProgressForm();
            progressForm.Text = string.Format("Progress loading {0}", fileName);
            progressForm.Visible = progressFormVisible;
            progressForm.setProgress(0);
            if (stacktraceTable == null)
            {
                stacktraceTable = new StacktraceTable();
            }

            if (timePos == null)
            {
                timePos = new TimePos[1000];
            }

            AddTypeName(0, "Free Space");
            try
            {
                Stream s = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                r = new StreamReader(s);
                for (timePosIndex = timePosCount; timePosIndex > 0; timePosIndex--)
                {
                    if (timePos[timePosIndex-1].pos <= startFileOffset)
                    {
                        break;
                    }
                }
                // start at the beginning if no later start point available or asked for info that can only
                // be constructed by reading the whole file.
                if (timePosIndex <= 1 || readLogResult.relocatedHistogram != null || readLogResult.finalizerHistogram != null
                                      || readLogResult.criticalFinalizerHistogram != null || readLogResult.liveObjectTable != null)
                {
                    pos = 0;
                    timePosIndex = 1;
                }
                else
                {
                    timePosIndex--;
                    pos = timePos[timePosIndex].pos;
                }
                if (timePosCount == 0)
                {
                    timePos[0] = new TimePos(0.0, 0);
                    timePosCount = timePosIndex = 1;
                }
                s.Position = pos;
                buffer = new byte[4096];
                bufPos = 0;
                bufLevel = 0;
                int maxProgress = (int)(r.BaseStream.Length/1024);
                progressForm.setMaximum(maxProgress);
                line = 1;
                var sb = new StringBuilder();
                ulong[] ulongStack = new ulong[1000];
                int[] intStack = new int[1000];
                int stackPtr = 0;
                c = ReadChar();
                bool thisIsR = false;
                bool extendedRootInfoSeen = false;
                int lastTickIndex = 0;
                bool newGcEvent = false;

                while (c != -1)
                {
                    if (pos > endFileOffset)
                    {
                        break;
                    }

                    if ((line % 1024) == 0)
                    {
                        int currentProgress = (int)(pos/1024);
                        if (currentProgress <= maxProgress)
                        {
                            progressForm.setProgress(currentProgress);
                            Application.DoEvents();
                            if (progressForm.DialogResult == DialogResult.Cancel)
                            {
                                break;
                            }
                        }
                    }
                    lastLineStartPos = pos-1;
                    bool previousWasR = thisIsR;
                    thisIsR = false;
                    switch (c)
                    {
                        case    -1:
                            break;

                        case    'F':
                        case    'f':
                        {
                            c = ReadChar();
                            int funcIndex = ReadInt();
                            while (c == ' ' || c == '\t')
                                {
                                    c = ReadChar();
                                }

                                string name = ReadString(sb, ' ', false, 255);
                            while (c == ' ' || c == '\t')
                                {
                                    c = ReadChar();
                                }

                                string signature = ReadString(sb, '\r', true, 1023);

                            ulong addr = ReadULong();
                            uint size = ReadUInt();
                            int modIndex = ReadInt();
                            int stackIndex = ReadInt();

                            if (c != -1)
                            {
                                EnsureStringCapacity(funcIndex, ref funcName);
                                funcName[funcIndex] = name;
                                EnsureStringCapacity(funcIndex, ref funcSignature);
                                funcSignature[funcIndex] = signature;
                                EnsureIntCapacity(funcIndex, ref funcModule);
                                funcModule[funcIndex] = modIndex;

                                string nameAndSignature = name;
                                if (signature != null)
                                    {
                                        nameAndSignature = name + " " +signature;
                                    }

                                    if (stackIndex >= 0 && readLogResult.functionList != null)
                                {
                                    funcSignatureIdHash[nameAndSignature] = funcIndex;
                                    readLogResult.functionList.Add(funcIndex, stackIndex, size, modIndex);
                                }
                            }
                            break;
                        }

                        case    'T':
                        case    't':
                        {
                            c = ReadChar();
                            int typeIndex = ReadInt();
                            while (c == ' ' || c == '\t')
                                {
                                    c = ReadChar();
                                }

                                if (c != -1 && Char.IsDigit((char)c))
                            {
                                if (ReadInt() != 0)
                                {
                                    finalizableTypes[typeIndex] = true;
                                }
                            }
                            while (c == ' ' || c == '\t')
                                {
                                    c = ReadChar();
                                }

                                string typeName = ReadString(sb, '\r', false, 1023);
                            if (c != -1)
                            {
                                AddTypeName(typeIndex, typeName);
                            }
                            break;
                        }

                        // 'A' with thread identifier
                        case    '!':
                        {
                            c = ReadChar();
                            int threadId = ReadInt();
                            ulong id = ReadULong();
                            int typeSizeStackTraceIndex = ReadInt();
                            typeSizeStackTraceIndex = stacktraceTable.MapTypeSizeStacktraceId(typeSizeStackTraceIndex);
                            if (c != -1)
                            {
                                if (readLogResult.liveObjectTable != null)
                                    {
                                        readLogResult.liveObjectTable.InsertObject(id, typeSizeStackTraceIndex, lastTickIndex, lastTickIndex, true, readLogResult.sampleObjectTable);
                                    }

                                    if (pos >= startFileOffset && pos < endFileOffset && readLogResult.allocatedHistogram != null)
                                {
                                    // readLogResult.calls.Add(new CallOrAlloc(false, threadId, typeSizeStackTraceIndex));
                                    readLogResult.allocatedHistogram.AddObject(typeSizeStackTraceIndex, 1);
                                }
                                List<string> prev;
                                if (assembliesJustLoaded.TryGetValue(threadId, out prev) && prev.Count != 0)
                                {
                                    foreach(string assemblyName in prev)
                                    {
                                        assemblies[assemblyName] = -typeSizeStackTraceIndex;
                                    }
                                    prev.Clear();
                                }
                            }
                            readLogResult.hadAllocInfo = true;
                            readLogResult.hadCallInfo = true;
                            break;
                        }

                        case    'A':
                        case    'a':
                        {
                            c = ReadChar();
                            ulong id = ReadULong();
                            int typeSizeStackTraceIndex = ReadInt();
                            typeSizeStackTraceIndex = stacktraceTable.MapTypeSizeStacktraceId(typeSizeStackTraceIndex);
                            if (c != -1)
                            {
                                if (readLogResult.liveObjectTable != null)
                                    {
                                        readLogResult.liveObjectTable.InsertObject(id, typeSizeStackTraceIndex, lastTickIndex, lastTickIndex, true, readLogResult.sampleObjectTable);
                                    }

                                    if (pos >= startFileOffset && pos < endFileOffset && readLogResult.allocatedHistogram != null)
                                {
                                    // readLogResult.calls.Add(new CallOrAlloc(false, typeSizeStackTraceIndex));
                                    readLogResult.allocatedHistogram.AddObject(typeSizeStackTraceIndex, 1);
                                }
                            }
                            readLogResult.hadAllocInfo = true;
                            readLogResult.hadCallInfo = true;
                            break;
                        }

                        case    'C':
                        case    'c':
                        {
                            c = ReadChar();
                            if (pos <  startFileOffset || pos >= endFileOffset)
                            {
                                while (c >= ' ')
                                    {
                                        c = ReadChar();
                                    }

                                    break;
                            }
                            int threadIndex = ReadInt();
                            int stackTraceIndex = ReadInt();
                            if ((c != -1) && (threadIndex != -1) && (stackTraceIndex != -1))
                            {
                                stackTraceIndex = stacktraceTable.MapTypeSizeStacktraceId(stackTraceIndex);
                                readLogResult.callstackHistogram?.AddObject(stackTraceIndex, 1);
                                List<string> prev;
                                if (assembliesJustLoaded.TryGetValue(threadIndex, out prev) && prev.Count != 0)
                                {
                                    foreach(string assemblyName in prev)
                                    {
                                        assemblies[assemblyName] = stackTraceIndex;
                                    }
                                    prev.Clear();
                                }
                            }
                            readLogResult.hadCallInfo = true;
                            break;
                        }

                        case    'E':
                        case    'e':
                        {
                            c = ReadChar();
                            extendedRootInfoSeen = true;
                            thisIsR = true;
                            if (pos <  startFileOffset || pos >= endFileOffset)
                            {
                                while (c >= ' ')
                                    {
                                        c = ReadChar();
                                    }

                                    break;
                            }
                            if (!previousWasR)
                            {
                                heapDumpEventList.AddEvent(lastTickIndex, null);
                                if (!readLogResult.objectGraph?.empty == true)
                                {
                                    readLogResult.objectGraph.BuildTypeGraph(new FilterForm());
                                    readLogResult.objectGraph.Neuter();
                                }
                                Histogram[] h = readLogResult.heapDumpHistograms;
                                if (h != null)
                                {
                                    if (h.Length == requestedIndex)
                                        {
                                            readLogResult.requestedObjectGraph = readLogResult.objectGraph;
                                        }

                                        readLogResult.heapDumpHistograms = new Histogram[h.Length + 1];
                                    for (int i = 0; i < h.Length; i++)
                                        {
                                            readLogResult.heapDumpHistograms[i] = h[i];
                                        }

                                        readLogResult.heapDumpHistograms[h.Length] = new Histogram(this, lastTickIndex);
                                }
                                readLogResult.objectGraph = new ObjectGraph(this, lastTickIndex);
                            }
                            ulong objectID = ReadULong();
                            var rootKind = (GcRootKind)ReadInt();
                            var rootFlags = (GcRootFlags)ReadInt();
                            ulong rootID = ReadULong();
                            ObjectGraph objectGraph = readLogResult.objectGraph;
                            if (c != -1 && objectID > 0 && objectGraph != null && (rootFlags & GcRootFlags.WeakRef) == 0)
                            {
                                string rootName;
                                switch (rootKind)
                                {
                                case    GcRootKind.Stack:      rootName = "Stack";        break;
                                case    GcRootKind.Finalizer:  rootName = "Finalizer";    break;
                                case    GcRootKind.Handle:     rootName = "Handle";       break;
                                default:                       rootName = "Other";        break;                      
                                }

                                if ((rootFlags & GcRootFlags.Pinning) != 0)
                                    {
                                        rootName += ", Pinning";
                                    }

                                    if ((rootFlags & GcRootFlags.WeakRef) != 0)
                                    {
                                        rootName += ", WeakRef";
                                    }

                                    if ((rootFlags & GcRootFlags.Interior) != 0)
                                    {
                                        rootName += ", Interior";
                                    }

                                    if ((rootFlags & GcRootFlags.Refcounted) != 0)
                                    {
                                        rootName += ", RefCounted";
                                    }

                                    int rootTypeId = objectGraph.GetOrCreateGcType(rootName);
                                ulongStack[0] = objectID;
                                ObjectGraph.GcObject rootObject = objectGraph.CreateObject(rootTypeId, 1, ulongStack);

                                objectGraph.AddRootObject(rootObject, rootID);
                            }
                            break;
                        }
                            
                        case    'R':
                        case    'r':
                        {
                            c = ReadChar();
                            thisIsR = true;
                            if (extendedRootInfoSeen || pos <  startFileOffset || pos >= endFileOffset)
                            {
                                while (c >= ' ')
                                    {
                                        c = ReadChar();
                                    }

                                    Histogram[] h = readLogResult.heapDumpHistograms;
                                if (h?.Length == requestedIndex)
                                {
                                    readLogResult.requestedObjectGraph = readLogResult.objectGraph;
                                }
                                break;
                            }
                            if (!previousWasR)
                            {
                                heapDumpEventList.AddEvent(lastTickIndex, null);
                                if (!readLogResult.objectGraph?.empty == true)
                                {
                                    readLogResult.objectGraph.BuildTypeGraph(new FilterForm());
                                    readLogResult.objectGraph.Neuter();
                                }
                                Histogram[] h = readLogResult.heapDumpHistograms;
                                if (h != null)
                                {
                                    if (h.Length == requestedIndex)
                                        {
                                            readLogResult.requestedObjectGraph = readLogResult.objectGraph;
                                        }

                                        readLogResult.heapDumpHistograms = new Histogram[h.Length + 1];
                                    for (int i = 0; i < h.Length; i++)
                                        {
                                            readLogResult.heapDumpHistograms[i] = h[i];
                                        }

                                        readLogResult.heapDumpHistograms[h.Length] = new Histogram(this, lastTickIndex);
                                }
                                readLogResult.objectGraph = new ObjectGraph(this, lastTickIndex);
                            }
                            stackPtr = 0;
                            ulong objectID;
                            while ((objectID = ReadULong()) != ulong.MaxValue)
                            {
                                if (objectID > 0)
                                {
                                    ulongStack[stackPtr] = objectID;
                                    stackPtr++;
                                    if (stackPtr >= ulongStack.Length)
                                        {
                                            ulongStack = GrowULongVector(ulongStack);
                                        }
                                    }
                            }
                            if (c != -1)
                            {
                                readLogResult.objectGraph?.AddRoots(stackPtr, ulongStack);
                            }
                            break;
                        }

                        case    'O':
                        case    'o':
                        {
                            c = ReadChar();
                            if (pos <  startFileOffset || pos >= endFileOffset || readLogResult.objectGraph == null)
                            {
                                while (c >= ' ')
                                    {
                                        c = ReadChar();
                                    }

                                    break;
                            }
                            ulong objectId = ReadULong();
                            int typeIndex = ReadInt();
                            uint size = ReadUInt();
                            stackPtr = 0;
                            ulong objectID;
                            while ((objectID = ReadULong()) != ulong.MaxValue)
                            {
                                if (objectID > 0)
                                {
                                    ulongStack[stackPtr] = objectID;
                                    stackPtr++;
                                    if (stackPtr >= ulongStack.Length)
                                        {
                                            ulongStack = GrowULongVector(ulongStack);
                                        }
                                    }
                            }
                            if (c != -1)
                            {
                                ObjectGraph objectGraph = readLogResult.objectGraph;
                                objectGraph.GetOrCreateGcType(typeIndex);

                                int typeSizeStackTraceId = -1;
                                int allocTickIndex = 0;
                                // try to find the allocation stack trace and allocation time
                                // from the live object table
                                if (readLogResult.liveObjectTable != null)
                                {
                                    LiveObjectTable.LiveObject liveObject;
                                    readLogResult.liveObjectTable.GetNextObject(objectId, objectId, out liveObject);
                                    if (liveObject.id == objectId)
                                    {
                                        int[] stackTrace = stacktraceTable.IndexToStacktrace(liveObject.typeSizeStacktraceIndex);
                                        int typeIndexFromLiveObject = stackTrace[0];
                                        if (typeIndexFromLiveObject == typeIndex)
                                        {
                                            typeSizeStackTraceId = liveObject.typeSizeStacktraceIndex;
                                            allocTickIndex = liveObject.allocTickIndex;
                                            Histogram[] h = readLogResult.heapDumpHistograms;
                                            if (h != null)
                                                {
                                                    h[h.Length - 1].AddObject(liveObject.typeSizeStacktraceIndex, 1);
                                                }
                                            }
                                    }
                                }
                                if (typeSizeStackTraceId == -1)
                                    {
                                        typeSizeStackTraceId = stacktraceTable.GetOrCreateTypeSizeId(typeIndex, (int)size);
                                    }

                                    ObjectGraph.GcObject gcObject = objectGraph.CreateAndEnterObject(objectId, typeSizeStackTraceId, stackPtr, ulongStack);
                                gcObject.AllocTickIndex = allocTickIndex;
                            }
                            break;
                        }

                        case    'M':
                        case    'm':
                        {
                            c = ReadChar();
                            int modIndex = ReadInt();
                            sb.Length = 0;
                            while (c > '\r')
                            {
                                sb.Append((char)c);
                                c = ReadChar();
                            }
                            if (c != -1)
                            {
                                string lineString = sb.ToString();
                                int addrPos = lineString.LastIndexOf(" 0x");
                                if (addrPos <= 0)
                                    {
                                        addrPos = lineString.Length;
                                    }

                                    int backSlashPos = lineString.LastIndexOf(@"\");
                                if (backSlashPos <= 0)
                                    {
                                        backSlashPos = -1;
                                    }

                                    string basicName = lineString.Substring(backSlashPos + 1, addrPos - backSlashPos - 1);
                                string fullName = lineString.Substring(0, addrPos);

                                EnsureStringCapacity(modIndex, ref modBasicName);
                                modBasicName[modIndex] = basicName;
                                EnsureStringCapacity(modIndex, ref modFullName);
                                modFullName[modIndex] = fullName;
                            }
                            break;
                        }

                        case    'U':
                        case    'u':
                        {
                            c = ReadChar();
                            ulong oldId = ReadULong();
                            ulong newId = ReadULong();
                            uint length = ReadUInt();
                            Histogram reloHist = null;
                            if (pos >= startFileOffset && pos < endFileOffset)
                                {
                                    reloHist = readLogResult.relocatedHistogram;
                                }

                                if (readLogResult.liveObjectTable != null)
                                {
                                    readLogResult.liveObjectTable.UpdateObjects(reloHist, oldId, newId, length, lastTickIndex, readLogResult.sampleObjectTable);
                                }

                                break;
                        }

                        case    'V':
                        case    'v':
                        {
                            c = ReadChar();
                            ulong startId = ReadULong();
                            uint length = ReadUInt();
                            Histogram reloHist = null;
                            if (pos >= startFileOffset && pos < endFileOffset)
                                {
                                    reloHist = readLogResult.relocatedHistogram;
                                }

                                if (readLogResult.liveObjectTable != null)
                                {
                                    readLogResult.liveObjectTable.UpdateObjects(reloHist, startId, startId, length, lastTickIndex, readLogResult.sampleObjectTable);
                                }

                                break;
                        }

                        case    'B':
                        case    'b':
                            c = ReadChar();
                            int startGC = ReadInt();
                            int induced = ReadInt();
                            int condemnedGeneration = ReadInt();
                            if (startGC != 0)
                            {
                                newGcEvent = gcEventList.AddEvent(lastTickIndex, null);
                            }

                            if (newGcEvent)
                            {
                                if (startGC != 0)
                                {
                                    if (induced != 0)
                                    {
                                        for (int gen = 0; gen <= condemnedGeneration; gen++)
                                        {
                                            inducedGcCount[gen]++;
                                        }
                                    }
                                }
                                else
                                {
                                    int condemnedLimit = condemnedGeneration;
                                    if (condemnedLimit == 2)
                                    {
                                        condemnedLimit = 3;
                                    }

                                    for (int gen = 0; gen <= condemnedLimit; gen++)
                                    {
                                        cumulativeGenerationSize[gen] += generationSize[gen];
                                        gcCount[gen]++;
                                    }
                                }
                            }

                            for (int gen = 0; gen <= 3; gen++)
                            {
                                generationSize[gen] = 0;
                            }

                            while (c >= ' ')
                            {
                                ulong rangeStart = ReadULong();
                                ulong rangeLength = ReadULong();
                                ulong rangeLengthReserved = ReadULong();
                                int rangeGeneration = ReadInt();
                                if (c == -1 || rangeGeneration < 0)
                                {
                                    break;
                                }

                                if (readLogResult.liveObjectTable != null)
                                {
                                    if (startGC != 0)
                                    {
                                        if (rangeGeneration > condemnedGeneration && condemnedGeneration != 2)
                                        {
                                            readLogResult.liveObjectTable.Preserve(rangeStart, rangeLength, lastTickIndex);
                                        }
                                    }
                                    else
                                    {
                                        readLogResult.liveObjectTable.GenerationInterval(rangeStart, rangeLength, rangeGeneration, lastTickIndex);
                                    }
                                }
                                generationSize[rangeGeneration] += rangeLength;
                            }
                            if (startGC == 0 && readLogResult.liveObjectTable != null)
                            {
                                readLogResult.liveObjectTable.RecordGc(lastTickIndex, condemnedGeneration, readLogResult.sampleObjectTable, false);
                            }
                            break;

                        case    'L':
                        case    'l':
                        {
                            c = ReadChar();
                            int isCritical = ReadInt();
                            ulong objectId = ReadULong();
                            if (pos >= startFileOffset && pos < endFileOffset && readLogResult.liveObjectTable != null)
                            {
                                // try to find the allocation stack trace and allocation time
                                // from the live object table
                                LiveObjectTable.LiveObject liveObject;
                                readLogResult.liveObjectTable.GetNextObject(objectId, objectId, out liveObject);
                                if (liveObject.id == objectId)
                                {
                                    if (isCritical != 0 && readLogResult.criticalFinalizerHistogram != null)
                                        {
                                            readLogResult.criticalFinalizerHistogram.AddObject(liveObject.typeSizeStacktraceIndex, 1);
                                        }

                                        if (readLogResult.finalizerHistogram != null)
                                        {
                                            readLogResult.finalizerHistogram.AddObject(liveObject.typeSizeStacktraceIndex, 1);
                                        }
                                    }
                            }
                            break;
                        }

                        case    'I':
                        case    'i':
                            c = ReadChar();
                            int tickCount = ReadInt();
                            if (c != -1)
                            {
                                lastTickIndex = AddTimePos(tickCount, lastLineStartPos);
                                if (maxTickIndex < lastTickIndex)
                                {
                                    maxTickIndex = lastTickIndex;
                                }
                            }
                            break;

                        case    'G':
                        case    'g':
                            c = ReadChar();
                            int gcGen0Count = ReadInt();
                            int gcGen1Count = ReadInt();
                            int gcGen2Count = ReadInt();
                            // if the newer 'b' lines occur, disregard the 'g' lines.
                            if (gcCount[0] == 0 && readLogResult.liveObjectTable != null)
                            {
                                if (c == -1 || gcGen0Count < 0)
                                {
                                    readLogResult.liveObjectTable.RecordGc(lastTickIndex, 0, readLogResult.sampleObjectTable, gcGen0Count < 0);
                                }
                                else
                                {
                                    readLogResult.liveObjectTable.RecordGc(lastTickIndex, gcGen0Count, gcGen1Count, gcGen2Count, readLogResult.sampleObjectTable);
                                }
                            }
                            break;

                        case    'N':
                        case    'n':
                        {
                            c = ReadChar();
                            int funcIndex;
                            int stackTraceIndex = ReadInt();
                            stackPtr = 0;

                            int flag = ReadInt();
                            int matched = flag / 4;
                            int hadTypeId = (flag & 2);
                            bool hasTypeId = (flag & 1) == 1;

                            if (hasTypeId)
                            {
                                intStack[stackPtr++] = ReadInt();
                                intStack[stackPtr++] = ReadInt();
                            }

                            if (matched > 0 && c != -1)
                            {
                                /* use some other stack trace as a reference */
                                int otherStackTraceId = ReadInt();
                                otherStackTraceId = stacktraceTable.MapTypeSizeStacktraceId(otherStackTraceId);
                                int[] stacktrace = stacktraceTable.IndexToStacktrace(otherStackTraceId);
                                if (matched > stacktrace.Length - hadTypeId)
                                    {
                                        matched = stacktrace.Length - hadTypeId;
                                    }

                                    for (int i = 0; i < matched; i++)
                                {
                                    int funcId = stacktrace[i + hadTypeId];
                                    Debug.Assert(funcId < funcName.Length);
                                    if (funcName[funcId] == null)
                                        {
                                            funcName[funcId] = String.Empty;
                                        }

                                        intStack[stackPtr++] = funcId;
                                    if (stackPtr >= intStack.Length)
                                    {
                                        intStack = GrowIntVector(intStack);
                                    }
                                }
                            }

                            while ((funcIndex = ReadInt()) >= 0)
                            {
                                intStack[stackPtr] = funcIndex;
                                stackPtr++;
                                if (stackPtr >= intStack.Length)
                                    {
                                        intStack = GrowIntVector(intStack);
                                    }
                                }

                            if (c != -1)
                            {
                                stacktraceTable.Add(stackTraceIndex, intStack, stackPtr, hasTypeId);
                            }
                            break;
                        }

                        case 'y':
                        case 'Y':
                        {
                            c = ReadChar();
                            int threadid = ReadInt();
                            if(!assembliesJustLoaded.ContainsKey(threadid))
                            {
                                assembliesJustLoaded[threadid] = new List<string>();
                            }
                            /* int assemblyId = */ ReadInt();

                            while (c == ' ' || c == '\t')
                            {
                                c = ReadChar();
                            }
                            sb.Length = 0;
                            while (c > '\r')
                            {
                                sb.Append((char)c);
                                c = ReadChar();
                            }
                            string assemblyName = sb.ToString();
                            assembliesJustLoaded[threadid].Add(assemblyName);
                            break;
                        }

                        case 'S':
                        case 's':
                        {
                            c = ReadChar();
                            int stackTraceIndex = ReadInt();
                            int funcIndex;
                            stackPtr = 0;
                            while ((funcIndex = ReadInt()) >= 0)
                            {
                                intStack[stackPtr] = funcIndex;
                                stackPtr++;
                                if (stackPtr >= intStack.Length)
                                    {
                                        intStack = GrowIntVector(intStack);
                                    }
                                }
                            if (c != -1)
                            {
                                stacktraceTable.Add(stackTraceIndex, intStack, stackPtr, false);
                            }
                            break;
                        }

                        case    'Z':
                        case    'z':
                        {
                            sb.Length = 0;
                            c = ReadChar();
                            while (c == ' ' || c == '\t')
                                {
                                    c = ReadChar();
                                }

                                while (c > '\r')
                            {
                                sb.Append((char)c);
                                c = ReadChar();
                            }
                            if (c != -1)
                            {
                                lastTickIndex = AddTimePos(lastLineStartPos);
                                if (maxTickIndex < lastTickIndex)
                                    {
                                        maxTickIndex = lastTickIndex;
                                    }

                                    commentEventList.AddEvent(lastTickIndex, sb.ToString());
                            }
                            break;
                        }

                        case    'H':
                        case    'h':
                        {
                            c = ReadChar();
                            int threadId = ReadInt();
                            ulong handleId = ReadULong();
                            ulong initialObjectId = ReadULong();
                            int stacktraceId = ReadInt();
                            if (c != -1)
                            {
                                if (readLogResult.handleHash != null)
                                    {
                                        readLogResult.handleHash[handleId] = new HandleInfo(threadId, handleId, initialObjectId, lastTickIndex, stacktraceId);
                                    }

                                    if (readLogResult.createdHandlesHistogram != null)
                                    {
                                        readLogResult.createdHandlesHistogram.AddObject(stacktraceId, 1);
                                    }
                                }
                            break;
                        }

                        case    'J':
                        case    'j':
                        {
                            c = ReadChar();
                            int threadId = ReadInt();
                            ulong handleId = ReadULong();
                            int stacktraceId = ReadInt();
                            if (c != -1)
                            {
                                if (readLogResult.handleHash != null)         
                                {
                                    if (readLogResult.handleHash.ContainsKey(handleId))
                                        {
                                            readLogResult.handleHash.Remove(handleId);
                                        }
                                        else
                                    {
                                        //Console.WriteLine("Non-existent handle {0:x} destroyed in line {1}", handleId, line);
                                        //int[] stacktrace = stacktraceTable.IndexToStacktrace(stacktraceId);
                                        //for (int i = stacktrace.Length; --i >= 0; )
                                        //{
                                        //    Console.WriteLine("  {0}", funcName[stacktrace[i]]);
                                        //}
                                    }
                                }
                                if (readLogResult.destroyedHandlesHistogram != null)
                                    {
                                        readLogResult.destroyedHandlesHistogram.AddObject(stacktraceId, 1);
                                    }
                                }
                            break;
                        }
                        
                        default:
                        {
                            // just ignore the unknown
                            while(c != '\n' && c != '\r')
                            {
                                c = ReadChar();
                            }
                            break;
                        }
                    }
                    while (c == ' ' || c == '\t')
                    {
                        c = ReadChar();
                    }

                    if (c == '\r')
                    {
                        c = ReadChar();
                    }

                    if (c == '\n')
                    {
                        c = ReadChar();
                        line++;
                    }
                }
//                readLogResult.functionList.ReportCallCountSizes(readLogResult.callstackHistogram);
            }
//            catch (Exception)
//            {
//                throw new Exception(string.Format("Bad format in log file {0} line {1}", fileName, line));
//                throw;
//            }
            finally
            {
                progressForm.Visible = false;
                progressForm.Dispose();
                if (r != null)
                {
                    r.Close();
                }
            }
        }
    }
}
