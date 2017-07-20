using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal sealed class SampleObjectTable
    {
        internal sealed class SampleObject
        {
            internal readonly int typeIndex;
            internal readonly int changeTickIndex;
            internal readonly int origAllocTickIndex;
            internal readonly SampleObject prev;

            internal SampleObject(int typeIndex, int changeTickIndex, int origAllocTickIndex, SampleObject prev)
            {
                this.typeIndex = typeIndex;
                this.changeTickIndex = changeTickIndex;
                this.origAllocTickIndex = origAllocTickIndex;
                this.prev = prev;
            }
        }

        internal SampleObject[][] masterTable;
        internal readonly ReadNewLog readNewLog;

        internal const int firstLevelShift = 25;
        internal const int initialFirstLevelLength = 1<<(31-firstLevelShift); // covering 2 GB of address space
        internal const int secondLevelShift = 10;
        internal const int secondLevelLength = 1<<(firstLevelShift-secondLevelShift);
        internal const int sampleGrain = 1<<secondLevelShift;
        internal int lastTickIndex;
        internal SampleObject gcTickList;

        private void GrowMasterTable()
        {
            SampleObject[][] newMasterTable = new SampleObject[masterTable.Length * 2][];
            for (int i = 0; i < masterTable.Length; i++)
            {
                newMasterTable[i] = masterTable[i];
            }

            masterTable = newMasterTable;
        }

        internal SampleObjectTable(ReadNewLog readNewLog)
        {
            masterTable = new SampleObject[initialFirstLevelLength][];
            this.readNewLog = readNewLog;
            lastTickIndex = 0;
            gcTickList = null;
        }

        private bool IsGoodSample(ulong start, ulong end)
        {
            // We want it as a sample if and only if it crosses a boundary
            return (start >> secondLevelShift) != (end >> secondLevelShift);
        }

        internal void RecordChange(ulong start, ulong end, int changeTickIndex, int origAllocTickIndex, int typeIndex)
        {
            lastTickIndex = changeTickIndex;
            for (ulong id = start; id < end; id += sampleGrain)
            {
                uint index = (uint)(id >> firstLevelShift);
                while (masterTable.Length <= index)
                {
                    GrowMasterTable();
                }

                SampleObject[] so = masterTable[index];
                if (so == null)
                {
                    so = new SampleObject[secondLevelLength];
                    masterTable[index] = so;
                }
                index = (uint)((id >> secondLevelShift) & (secondLevelLength-1));
                Debug.Assert(so[index] == null || so[index].changeTickIndex <= changeTickIndex);
                SampleObject prev = so[index];
                if (prev != null && prev.typeIndex == typeIndex && prev.origAllocTickIndex == origAllocTickIndex)
                {
                    // no real change - can happen when loading files where allocation profiling was off
                    // to conserve memory, don't allocate a new sample object in this case
                }
                else
                {
                    so[index] = new SampleObject(typeIndex, changeTickIndex, origAllocTickIndex, prev);
                }
            }
        }

        internal void Insert(ulong start, ulong end, int changeTickIndex, int origAllocTickIndex, int typeIndex)
        {
            if (IsGoodSample(start, end))
            {
                RecordChange(start, end, changeTickIndex, origAllocTickIndex, typeIndex);
            }
        }

        internal void Delete(ulong start, ulong end, int changeTickIndex)
        {
            if (IsGoodSample(start, end))
            {
                RecordChange(start, end, changeTickIndex, 0, 0);
            }
        }

        internal void AddGcTick(int tickIndex, int gen)
        {
            lastTickIndex = tickIndex;

            gcTickList = new SampleObject(gen, tickIndex, 0, gcTickList);
        }

        internal void RecordComment(int tickIndex, int commentIndex)
        {
            lastTickIndex = tickIndex;
        }
    }
}