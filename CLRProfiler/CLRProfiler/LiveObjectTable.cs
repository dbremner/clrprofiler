using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal sealed partial class LiveObjectTable
    {
        private readonly IntervalTable intervalTable;
        internal readonly ReadNewLog readNewLog;
        internal int lastTickIndex;
        private long lastPos;

        private const int alignShift = 2;
        private const int firstLevelShift = 20;
        private const int initialFirstLevelLength = 1 << (31 - alignShift - firstLevelShift);  // covering 2 GB of address space
        private const int secondLevelLength = 1<<firstLevelShift;
        private const int secondLevelMask = secondLevelLength-1;

        private ushort[][] firstLevelTable;

        private void GrowFirstLevelTable()
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

        private ulong FindObjectForward(ulong startId, ulong endId)
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

        private void Write3WordsAt(ulong id, ushort u1, ushort u2, ushort u3)
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

        private void RemoveObjectRange(ulong firstId, ulong length, int tickIndex, SampleObjectTable sampleObjectTable)
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