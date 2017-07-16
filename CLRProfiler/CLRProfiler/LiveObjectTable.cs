using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal class LiveObjectTable
    {
        internal struct LiveObject
        {
            internal ulong id;
            internal uint size;
            internal int typeIndex;
            internal int typeSizeStacktraceIndex;
            internal int allocTickIndex;
        }

        class IntervalTable
        {
            class Interval
            {
                internal ulong loAddr;
                internal ulong hiAddr;
                internal readonly int generation;
                internal Interval next;
                internal bool hadRelocations;
                internal bool justHadGc;

                internal Interval(ulong loAddr, ulong hiAddr, int generation, Interval next)
                {
                    this.loAddr = loAddr;
                    this.hiAddr = hiAddr;
                    this.generation = generation;
                    this.next = next;
                }
            }

            const int allowableGap = 1024*1024;

            Interval liveRoot;
            Interval newLiveRoot;
            Interval updateRoot;
            bool nullRelocationsSeen;

            readonly LiveObjectTable liveObjectTable;

            internal IntervalTable(LiveObjectTable liveObjectTable)
            {
                liveRoot = null;
                this.liveObjectTable = liveObjectTable;
            }

            private Interval OverlappingInterval(Interval i)
            {
                for (Interval ii = liveRoot; ii != null; ii = ii.next)
                {
                    if (ii != i)
                    {
                        if (ii.hiAddr > i.loAddr && ii.loAddr < i.hiAddr)
                        {
                            return ii;
                        }
                    }
                }
                return null;
            }

            private void DeleteInterval(Interval i)
            {   
                Interval prevInterval = null;
                for (Interval ii = liveRoot; ii != null; ii = ii.next)
                {
                    if (ii == i)
                    {
                        if (prevInterval != null)
                        {
                            prevInterval.next = ii.next;
                        }
                        else
                        {
                            liveRoot = ii.next;
                        }

                        break;
                    }
                    prevInterval = ii;
                }
            }

            private void MergeInterval(Interval i)
            {
                Interval overlappingInterval = OverlappingInterval(i);
                i.loAddr = Math.Min(i.loAddr, overlappingInterval.loAddr);
                i.hiAddr = Math.Max(i.hiAddr, overlappingInterval.hiAddr);
                DeleteInterval(overlappingInterval);
            }

            internal bool AddObject(ulong id, uint size, int allocTickIndex, SampleObjectTable sampleObjectTable)
            {
                size = (size + 3) & (uint.MaxValue - 3);
                Interval prevInterval = null;
                Interval bestInterval = null;
                Interval prevI = null;
                bool emptySpace = false;
                // look for the best interval to put this object in.
                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr < id + size && id <= i.hiAddr + allowableGap)
                    {
                        if (bestInterval == null || bestInterval.loAddr < i.loAddr)
                        {
                            bestInterval = i;
                            prevInterval = prevI;
                        }
                    }
                    prevI = i;
                }
                if (bestInterval != null)
                {
                    if (bestInterval.loAddr > id)
                    {
                        bestInterval.loAddr = id;
                    }
                    if (id < bestInterval.hiAddr)
                    {
                        if (bestInterval.hadRelocations && bestInterval.justHadGc)
                        {
                            // Interval gets shortened
                            liveObjectTable.RemoveObjectRange(id, bestInterval.hiAddr - id, allocTickIndex, sampleObjectTable);
                            bestInterval.hiAddr = id + size;
                            bestInterval.justHadGc = false;
                        }
                    }
                    else
                    {
                        bestInterval.hiAddr = id + size;
                        emptySpace = true;
                    }                   
                    if (prevInterval != null)
                    {
                        // Move to front to speed up future searches.
                        prevInterval.next = bestInterval.next;
                        bestInterval.next = liveRoot;
                        liveRoot = bestInterval;
                    }
                    if (OverlappingInterval(bestInterval) != null)
                    {
                        MergeInterval(bestInterval);
                    }

                    return emptySpace;
                }
                liveRoot = new Interval(id, id + size, -1, liveRoot);
                Debug.Assert(OverlappingInterval(liveRoot) == null);
                return false;
            }

            internal void GenerationInterval(ulong rangeStart, ulong rangeLength, int generation)
            {
                newLiveRoot = new Interval(rangeStart, rangeStart + rangeLength, generation, newLiveRoot);
            }

            internal int GenerationOfObject(ulong id)
            {
                Interval prev = null;
                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr <= id && id < i.hiAddr)
                    {
                        if (prev != null)
                        {
                            // Move to front to speed up future searches.
                            prev.next = i.next;
                            i.next = liveRoot;
                            liveRoot = i;
                        }
                        return i.generation;
                    }
                    prev = i;
                }
                return -1;
            }

            internal void Preserve(ulong id, ulong length)
            {
                if (updateRoot != null && updateRoot.hiAddr == id)
                {
                    updateRoot.hiAddr = id + length;
                }
                else
                {
                    updateRoot = new Interval(id, id + length, -1, updateRoot);
                }
            }

            internal void Relocate(ulong oldId, ulong newId, uint length)
            {
                if (oldId == newId)
                {
                    nullRelocationsSeen = true;
                }

                if (updateRoot != null && updateRoot.hiAddr == newId)
                {
                    updateRoot.hiAddr = newId + length;
                }
                else
                {
                    updateRoot = new Interval(newId, newId + length, -1, updateRoot);
                }

                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr <= oldId && oldId < i.hiAddr)
                    {
                        i.hadRelocations = true;
                    }
                }
                Interval bestInterval = null;
                for (Interval i = liveRoot; i != null; i = i.next)
                {
                    if (i.loAddr <= newId + length && newId <= i.hiAddr + allowableGap)
                    {
                        if (bestInterval == null || bestInterval.loAddr < i.loAddr)
                        {
                            bestInterval = i;
                        }
                    }
                }
                if (bestInterval != null)
                {
                    if (bestInterval.hiAddr < newId + length)
                    {
                        bestInterval.hiAddr = newId + length;
                    }

                    if (bestInterval.loAddr > newId)
                    {
                        bestInterval.loAddr = newId;
                    }

                    if (OverlappingInterval(bestInterval) != null)
                    {
                        MergeInterval(bestInterval);
                    }
                }
                else
                {
                    liveRoot = new Interval(newId, newId + length, -1, liveRoot);
                    Debug.Assert(OverlappingInterval(liveRoot) == null);
                }
            }

            private Interval SortIntervals(Interval root)
            {
                // using insertion sort for now...
                Interval next;
                Interval newRoot = null;
                for (Interval i = root; i != null; i = next)
                {
                    next = i.next;
                    Interval prev = null;
                    Interval ii;
                    for (ii = newRoot; ii != null; ii = ii.next)
                    {
                        if (i.loAddr < ii.loAddr)
                        {
                            break;
                        }

                        prev = ii;
                    }
                    if (prev == null)
                    {
                        i.next = newRoot;
                        newRoot = i;
                    }
                    else
                    {
                        i.next = ii;
                        prev.next = i;
                    }
                }
                return newRoot;
            }

            private void RemoveRange(ulong loAddr, ulong hiAddr, int tickIndex, SampleObjectTable sampleObjectTable)
            {
                Interval next;
                for (Interval i = liveRoot; i != null; i = next)
                {
                    next = i.next;
                    ulong lo = Math.Max(loAddr, i.loAddr);
                    ulong hi = Math.Min(hiAddr, i.hiAddr);
                    if (lo >= hi)
                    {
                        continue;
                    }

                    liveObjectTable.RemoveObjectRange(lo, hi - lo, tickIndex, sampleObjectTable);
                    if (i.hiAddr == hi)
                    {
                        if (i.loAddr == lo)
                        {
                            DeleteInterval(i);
                        }
                        else
                        {
                            i.hiAddr = lo;
                        }
                    }
                }
            }

            internal void RecordGc(int tickIndex, SampleObjectTable sampleObjectTable, bool simpleForm)
            {
                if (simpleForm && nullRelocationsSeen || newLiveRoot != null)
                {
                    // in this case assume anything not reported is dead
                    updateRoot = SortIntervals(updateRoot);
                    ulong prevHiAddr = 0;
                    for (Interval i = updateRoot; i != null; i = i.next)
                    {
                        if (prevHiAddr < i.loAddr)
                        {
                            RemoveRange(prevHiAddr, i.loAddr, tickIndex, sampleObjectTable);
                        }
                        if (prevHiAddr < i.hiAddr)
                        {
                            prevHiAddr = i.hiAddr;
                        }
                    }
                    RemoveRange(prevHiAddr, ulong.MaxValue, tickIndex, sampleObjectTable);
                    updateRoot = null;
                    if (newLiveRoot != null)
                    {
                        liveRoot = newLiveRoot;
                        newLiveRoot = null;
                    }
                }
                else
                {
                    for (Interval i = liveRoot; i != null; i = i.next)
                    {
                        i.justHadGc = true;
                    }
                }
                nullRelocationsSeen = false;
            }
        }

        readonly IntervalTable intervalTable;
        internal readonly ReadNewLog readNewLog;
        internal int lastTickIndex;
        private long lastPos;

        const int alignShift = 2;
        const int firstLevelShift = 20;
        const int initialFirstLevelLength = 1 << (31 - alignShift - firstLevelShift);  // covering 2 GB of address space
        const int secondLevelLength = 1<<firstLevelShift;
        const int secondLevelMask = secondLevelLength-1;

        ushort[][] firstLevelTable;

        void GrowFirstLevelTable()
        {
            ushort[][] newFirstLevelTable = new ushort[firstLevelTable.Length*2][];
            for (int i = 0; i < firstLevelTable.Length; i++)
            {
                newFirstLevelTable[i] = firstLevelTable[i];
            }

            firstLevelTable = newFirstLevelTable;
        }

        internal LiveObjectTable(ReadNewLog readNewLog)
        {
            firstLevelTable = new ushort[initialFirstLevelLength][];
            intervalTable = new IntervalTable(this);
            this.readNewLog = readNewLog;
            lastGcGen0Count = 0;
            lastGcGen1Count = 0;
            lastGcGen2Count = 0;
            lastTickIndex = 0;
            lastPos = 0;
        }

        internal ulong FindObjectBackward(ulong id)
        {
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            if (i >= firstLevelTable.Length)
            {
                i = (uint)(firstLevelTable.Length - 1);
                j = (uint)(secondLevelLength - 1);
            }
            while (i != uint.MaxValue)
            {
                ushort[] secondLevelTable = firstLevelTable[i];
                if (secondLevelTable != null)
                {
                    while (j != uint.MaxValue)
                    {
                        if ((secondLevelTable[j] & 0x8000) != 0)
                        {
                            break;
                        }

                        j--;
                    }
                    if (j != uint.MaxValue)
                    {
                        break;
                    }
                }
                j = secondLevelLength - 1;
                i--;
            }
            if (i == uint.MaxValue)
            {
                return 0;
            }
            else
            {
                return (((ulong)i<<firstLevelShift) + j) << alignShift;
            }
        }

        ulong FindObjectForward(ulong startId, ulong endId)
        {
            startId >>= alignShift;
            endId >>= alignShift;
            uint i = (uint)(startId >> firstLevelShift);
            uint iEnd = (uint)(endId >> firstLevelShift);
            uint j = (uint)(startId & secondLevelMask);
            uint jEnd = (uint)(endId & secondLevelMask);
            if (iEnd >= firstLevelTable.Length)
            {
                iEnd = (uint)(firstLevelTable.Length - 1);
                jEnd = (uint)(secondLevelLength - 1);
            }
            while (i <= iEnd)
            {
                ushort[] secondLevelTable = firstLevelTable[i];
                if (secondLevelTable != null)
                {
                    while (j < secondLevelLength && (j <= jEnd || i < iEnd))
                    {
                        if ((secondLevelTable[j] & 0x8000) != 0)
                        {
                            break;
                        }

                        j++;
                    }
                    if (j < secondLevelLength)
                    {
                        break;
                    }
                }
                j = 0;
                i++;
            }
            if (i > iEnd || (i == iEnd && j > jEnd))
            {
                return ulong.MaxValue;
            }
            else
            {
                return (((ulong)i<<firstLevelShift) + j) << alignShift;
            }
        }

        internal void GetNextObject(ulong startId, ulong endId, out LiveObject o)
        {
            ulong id = FindObjectForward(startId, endId);
            o.id = id;
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            ushort[] secondLevelTable = null;
            if (i < firstLevelTable.Length)
            {
                secondLevelTable = firstLevelTable[i];
            }

            if (secondLevelTable != null)
            {
                ushort u1 = secondLevelTable[j];
                if ((u1 & 0x8000) != 0)
                {
                    j++;
                    if (j >= secondLevelLength)
                    {
                        j = 0;
                        i++;
                        secondLevelTable = firstLevelTable[i];
                    }
                    ushort u2 = secondLevelTable[j];
                    j++;
                    if (j >= secondLevelLength)
                    {
                        j = 0;
                        i++;
                        secondLevelTable = firstLevelTable[i];
                    }
                    ushort u3 = secondLevelTable[j];

                    o.allocTickIndex = (u2 >> 7) + (u3 << 8);

                    o.typeSizeStacktraceIndex = (u1 & 0x7fff) + ((u2 & 0x7f) << 15);

                    int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(o.typeSizeStacktraceIndex);

                    o.typeIndex = stacktrace[0];
                    o.size = (uint)stacktrace[1];

                    return;
                }
            }
            o.size = 0;
            o.allocTickIndex = o.typeIndex = o.typeSizeStacktraceIndex = 0;
        }

        void Write3WordsAt(ulong id, ushort u1, ushort u2, ushort u3)
        {
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            while (firstLevelTable.Length <= i+1)
            {
                GrowFirstLevelTable();
            }

            ushort[] secondLevelTable = firstLevelTable[i];
            if (secondLevelTable == null)
            {
                secondLevelTable = new ushort[secondLevelLength];
                firstLevelTable[i] = secondLevelTable;
            }
            secondLevelTable[j] = u1;
            j++;
            if (j >= secondLevelLength)
            {
                j = 0;
                i++;
                secondLevelTable = firstLevelTable[i];
                if (secondLevelTable == null)
                {
                    secondLevelTable = new ushort[secondLevelLength];
                    firstLevelTable[i] = secondLevelTable;
                }
            }
            secondLevelTable[j] = u2;
            j++;
            if (j >= secondLevelLength)
            {
                j = 0;
                i++;
                secondLevelTable = firstLevelTable[i];
                if (secondLevelTable == null)
                {
                    secondLevelTable = new ushort[secondLevelLength];
                    firstLevelTable[i] = secondLevelTable;
                }
            }
            secondLevelTable[j] = u3;
        }

        internal void Zero(ulong id, uint size)
        {
            uint count = ((size + 3) & (uint.MaxValue - 3))/4;
            id >>= alignShift;
            uint i = (uint)(id >> firstLevelShift);
            uint j = (uint)(id & secondLevelMask);
            ushort[] secondLevelTable = null;
            if (i < firstLevelTable.Length)
            {
                secondLevelTable = firstLevelTable[i];
            }

            while (count > 0)
            {
                // Does the piece to clear fit within the secondLevelTable?
                if (j + count <= secondLevelLength)
                {
                    // yes - if there is no secondLevelTable, there is nothing left to do
                    if (secondLevelTable == null)
                    {
                        break;
                    }

                    while (count > 0)
                    {
                        secondLevelTable[j] = 0;
                        count--;
                        j++;
                    }
                }
                else
                {
                    // no - if there is no secondLevelTable, skip it
                    if (secondLevelTable == null)
                    {
                        count -= secondLevelLength - j;
                    }
                    else
                    {
                        while (j < secondLevelLength)
                        {
                            secondLevelTable[j] = 0;
                            count--;
                            j++;
                        }
                    }
                    j = 0;
                    i++;
                    secondLevelTable = null;
                    if (i < firstLevelTable.Length)
                    {
                        secondLevelTable = firstLevelTable[i];
                    }
                }
            }
        }

        internal void Zero(ulong id, ulong size)
        {
            while (size >= uint.MaxValue)
            {
                Zero(id, uint.MaxValue);
                id += uint.MaxValue;
                size -= uint.MaxValue;
            }
            Zero(id, (uint)size);
        }

        internal bool CanReadObjectBackCorrectly(ulong id, uint size, int typeSizeStacktraceIndex, int allocTickIndex)
        {
            LiveObject o;
            GetNextObject(id, id + size, out o);
            return o.id == id && o.typeSizeStacktraceIndex == typeSizeStacktraceIndex && o.allocTickIndex == allocTickIndex;
        }

        internal void InsertObject(ulong id, int typeSizeStacktraceIndex, int allocTickIndex, int nowTickIndex, bool newAlloc, SampleObjectTable sampleObjectTable)
        {
            if (lastPos >= readNewLog.pos && newAlloc)
            {
                return;
            }

            lastPos = readNewLog.pos;

            lastTickIndex = nowTickIndex;
            int[] stacktrace = readNewLog.stacktraceTable.IndexToStacktrace(typeSizeStacktraceIndex);
            int typeIndex = stacktrace[0];
            uint size = (uint)stacktrace[1];
            bool emptySpace = false;
            if (newAlloc)
            {
                emptySpace = intervalTable.AddObject(id, size, allocTickIndex, sampleObjectTable);
            }
            if (!emptySpace)
            {
                ulong prevId = FindObjectBackward(id - 4);
                LiveObject o;
                GetNextObject(prevId, id, out o);
                if (o.id < id && (o.id + o.size > id || o.id + 12 > id))
                {
                    Zero(o.id, id - o.id);
                }
            }
            Debug.Assert(FindObjectBackward(id-4)+12 <= id);
            if (size >= 12)
            {
                ushort u1 = (ushort)(typeSizeStacktraceIndex | 0x8000);
                ushort u2 = (ushort)((typeSizeStacktraceIndex >> 15) | ((allocTickIndex & 0xff) << 7));
                ushort u3 = (ushort)(allocTickIndex >> 8);
                Write3WordsAt(id, u1, u2, u3);
                if (!emptySpace)
                {
                    Zero(id + 12, size - 12);
                }

                Debug.Assert(CanReadObjectBackCorrectly(id, size, typeSizeStacktraceIndex, allocTickIndex));
            }
            sampleObjectTable?.Insert(id, id + size, nowTickIndex, allocTickIndex, typeIndex);
        }

        void RemoveObjectRange(ulong firstId, ulong length, int tickIndex, SampleObjectTable sampleObjectTable)
        {
            ulong lastId = firstId + length;

            sampleObjectTable?.Delete(firstId, lastId, tickIndex);

            Zero(firstId, length);
        }

        internal void GenerationInterval(ulong rangeStart, ulong rangeLength, int generation, int tickIndex)
        {
            lastPos = readNewLog.pos;

            lastTickIndex = tickIndex;
            intervalTable.GenerationInterval(rangeStart, rangeLength, generation);            
        }

        internal int GenerationOfObject(ref LiveObject o)
        {
            int generation = intervalTable.GenerationOfObject(o.id);
            if (generation < 0)
            {
                generation = 0;
                if (o.allocTickIndex <= gen2LimitTickIndex)
                {
                    generation = 2;
                }
                else if (o.allocTickIndex <= gen1LimitTickIndex)
                {
                    generation = 1;
                }
            }
            return generation;
        }

        internal void Preserve(ulong id, ulong length, int tickIndex)
        {
            if (lastPos >= readNewLog.pos)
            {
                return;
            }

            lastPos = readNewLog.pos;

            lastTickIndex = tickIndex;
            intervalTable.Preserve(id, length);            
        }

        internal void UpdateObjects(Histogram relocatedHistogram, ulong oldId, ulong newId, uint length, int tickIndex, SampleObjectTable sampleObjectTable)
        {
            if (lastPos >= readNewLog.pos)
            {
                return;
            }

            lastPos = readNewLog.pos;

            lastTickIndex = tickIndex;
            intervalTable.Relocate(oldId, newId, length);

            if (oldId == newId)
            {
                return;
            }

            ulong nextId;
            ulong lastId = oldId + length;
            LiveObject o;
            for (GetNextObject(oldId, lastId, out o); o.id < lastId; GetNextObject(nextId, lastId, out o))
            {
                nextId = o.id + o.size;
                ulong offset = o.id - oldId;
                sampleObjectTable?.Delete(o.id, o.id + o.size, tickIndex);

                Zero(o.id, o.size);
                InsertObject(newId + offset, o.typeSizeStacktraceIndex, o.allocTickIndex, tickIndex, false, sampleObjectTable);
                relocatedHistogram?.AddObject(o.typeSizeStacktraceIndex, 1);
            }
        }

        internal int lastGcGen0Count;
        internal int lastGcGen1Count;
        internal int lastGcGen2Count;

        internal int gen1LimitTickIndex;
        internal int gen2LimitTickIndex;

        internal void RecordGc(int tickIndex, int gen, SampleObjectTable sampleObjectTable, bool simpleForm)
        {
            lastTickIndex = tickIndex;

            sampleObjectTable?.AddGcTick(tickIndex, gen);

            intervalTable.RecordGc(tickIndex, sampleObjectTable, simpleForm);

            if (gen >= 1)
            {
                gen2LimitTickIndex = gen1LimitTickIndex;
            }

            gen1LimitTickIndex = tickIndex;

            lastGcGen0Count++;
            if (gen > 0)
            {
                lastGcGen1Count++;
                if (gen > 1)
                {
                    lastGcGen2Count++;
                }
            }
        }

        internal void RecordGc(int tickIndex, int gcGen0Count, int gcGen1Count, int gcGen2Count, SampleObjectTable sampleObjectTable)
        {
            int gen = 0;
            if (gcGen2Count != lastGcGen2Count)
            {
                gen = 2;
            }
            else if (gcGen1Count != lastGcGen1Count)
            {
                gen = 1;
            }

            RecordGc(tickIndex, gen, sampleObjectTable, false);

            lastGcGen0Count = gcGen0Count;
            lastGcGen1Count = gcGen1Count;
            lastGcGen2Count = gcGen2Count;
        }
    }
}