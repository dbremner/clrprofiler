using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal class StacktraceTable
    {
        private int[][] stacktraceTable;
        private int[] mappingTable;
        private int maxID = 0;
        // logs created by the debugger have lots of duplicate type id/size combinations
        private readonly Dictionary<long, int> allocHashtable;

        internal StacktraceTable()
        {
            stacktraceTable = new int[1000][];
            stacktraceTable[0] = new int[0];
            allocHashtable = new Dictionary<long, int>();
        }

        private int LookupAlloc(int typeId, int size)
        {
            long key = typeId + ((long)size << 32);
            int data;
            if (allocHashtable.TryGetValue(key, out data))
            {
                return data;
            }
            return -1;
        }

        internal int MapTypeSizeStacktraceId(int id)
        {
            if (mappingTable != null)
            {
                return mappingTable[id];
            }

            return id;
        }

        private void EnterAlloc(int id, int typeId, int size)
        {
            long key = typeId + ((long)size << 32);
            allocHashtable[key] = id;
        }

        internal void Add(int id, int[] stack, int length, bool isAllocStack)
        {
            Add( id, stack, 0, length, isAllocStack );
        }

        private void CreateMappingTable()
        {
            mappingTable = new int[stacktraceTable.Length];
            for (int i = 0; i < mappingTable.Length; i++)
            {
                mappingTable[i] = i;
            }
        }

        private void GrowMappingTable()
        {
            int[] newMappingTable = new int[mappingTable.Length*2];
            for (int i = 0; i < mappingTable.Length; i++)
            {
                newMappingTable[i] = mappingTable[i];
            }

            mappingTable = newMappingTable;
        }

        internal void Add(int id, int[] stack, int start, int length, bool isAllocStack)
        {
            int oldId = -1;
            if (isAllocStack && length == 2)
            {
                oldId = LookupAlloc(stack[start], stack[start+1]);
            }

            if (oldId >= 0)
            {
                if (mappingTable == null)
                {
                    CreateMappingTable();
                }

                while (mappingTable.Length <= id)
                {
                    GrowMappingTable();
                }

                mappingTable[id] = oldId;
            }
            else
            {
                int[] stacktrace = new int[length];
                for (int i = 0; i < stacktrace.Length; i++)
                {
                    stacktrace[i] = stack[start++];
                }

                if (mappingTable != null)
                {
                    int newId = maxID + 1;
                    while (mappingTable.Length <= id)
                    {
                        GrowMappingTable();
                    }

                    mappingTable[id] = newId;
                    id = newId;
                }

                if (isAllocStack && length == 2)
                {
                    EnterAlloc(id, stacktrace[0], stacktrace[1]);
                }

                while (stacktraceTable.Length <= id)
                {
                    int[][] newStacktraceTable = new int[stacktraceTable.Length*2][];
                    for (int i = 0; i < stacktraceTable.Length; i++)
                    {
                        newStacktraceTable[i] = stacktraceTable[i];
                    }

                    stacktraceTable = newStacktraceTable;
                }

                stacktraceTable[id] = stacktrace;

                if (id > maxID)
                {
                    maxID = id;
                }
            }
        }

        internal int GetOrCreateTypeSizeId(int typeId, int size)
        {
            int id = LookupAlloc(typeId, size);
            if (id > 0)
            {
                return id;
            }

            if (mappingTable == null)
            {
                CreateMappingTable();
            }

            id = ++maxID;

            EnterAlloc(id, typeId, size);

            while (stacktraceTable.Length <= id)
            {
                int[][] newStacktraceTable = new int[stacktraceTable.Length * 2][];
                for (int i = 0; i < stacktraceTable.Length; i++)
                {
                    newStacktraceTable[i] = stacktraceTable[i];
                }

                stacktraceTable = newStacktraceTable;
            }

            int[] stacktrace = new int[2];
            stacktrace[0] = typeId;
            stacktrace[1] = size;
            stacktraceTable[id] = stacktrace;

            return id;
        }

        internal int[] IndexToStacktrace(int index)
        {
            if (index < 0 || index >= stacktraceTable.Length || stacktraceTable[index] == null)
            {
                Console.WriteLine("bad index {0}", index);
            }

            return stacktraceTable[index];
        }

        internal void FreeEntries( int firstIndex )
        {
            maxID = firstIndex;
        }

        internal int Length => maxID + 1;
    }
}