using System;
using System.Diagnostics;

namespace CLRProfiler
{
    internal sealed partial class LiveObjectTable
    {
        private sealed partial class IntervalTable
        {
            private const int allowableGap = 1024*1024;

            private Interval liveRoot;
            private Interval newLiveRoot;
            private Interval updateRoot;
            private bool nullRelocationsSeen;

            private readonly LiveObjectTable liveObjectTable;

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
    }
}