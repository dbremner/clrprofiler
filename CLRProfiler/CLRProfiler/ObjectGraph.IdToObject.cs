// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;

namespace CLRProfiler
{
    public sealed partial class ObjectGraph
    {
        internal sealed class IdToObject
        {
            internal const int lowAddressBits = 25; // one subtable for 32 MB of address space
            internal const int lowAddressMask = (1 << lowAddressBits) - 1;
            internal const int bucketBits = 9; // one bucket for 512 bytes of address space
            internal const int bucketSize = (1 << bucketBits);
            internal const int bucketMask = bucketBits - 1;
            internal const int alignBits = 2; // assume at least DWORD alignment so the lower two bits don't have to be represented
            internal const int idBits = bucketBits - alignBits; // therefore only 7 bits have to be stored in the object
            internal const int idMask = (1 << idBits) - 1;

            private GcObject[][] masterTable;

            internal IdToObject()
            {
                masterTable = new GcObject[1][];
            }

            internal void GrowMasterTable()
            {
                GcObject[][] newmasterTable = new GcObject[masterTable.Length * 2][];
                for (int i = 0; i < masterTable.Length; i++)
                {
                    newmasterTable[i] = masterTable[i];
                }

                masterTable = newmasterTable;
            }

            internal GcObject this[ulong objectID]
            {
                get
                {
                    int lowBits = (int)(objectID & lowAddressMask);
                    int highBits = (int)(objectID >> lowAddressBits);
                    if (highBits >= masterTable.Length)
                    {
                        return null;
                    }

                    GcObject[] subTable = masterTable[highBits];
                    if (subTable == null)
                    {
                        return null;
                    }

                    int bucket = lowBits >> bucketBits;
                    lowBits = (lowBits >> alignBits) & idMask;
                    GcObject o = subTable[bucket];
                    while (o != null && o.Id != lowBits)
                    {
                        o = o.nextInHash;
                    }

                    return o;
                }
                set
                {
                    int lowBits = (int)(objectID & lowAddressMask);
                    int highBits = (int)(objectID >> lowAddressBits);
                    while (highBits >= masterTable.Length)
                    {
                        GrowMasterTable();
                    }
                    GcObject[] subTable = masterTable[highBits];
                    if (subTable == null)
                    {
                        masterTable[highBits] = subTable = new GcObject[1 << (lowAddressBits - bucketBits)];
                    }
                    int bucket = lowBits >> bucketBits;
                    lowBits = (lowBits >> alignBits) & idMask;
                    value.Id = lowBits;
                    value.nextInHash = subTable[bucket];
                    subTable[bucket] = value;
                }
            }

            public IEnumerator<KeyValuePair<ulong, GcObject>> GetEnumerator()
            {
                for (int i = 0; i < masterTable.Length; i++)
                {
                    GcObject[] subTable = masterTable[i];
                    if (subTable == null)
                    {
                        continue;
                    }

                    for (int j = 0; j < subTable.Length; j++)
                    {
                        for (GcObject gcObject = subTable[j]; gcObject != null; gcObject = gcObject.nextInHash)
                        {
                            yield return new KeyValuePair<ulong, GcObject>(((ulong)i<<lowAddressBits)+ 
                                                                           ((ulong)j << bucketBits) +
                                                                           ((ulong)gcObject.Id << alignBits), gcObject);
                        }
                    }
                }
            }

            public IEnumerable<GcObject> Values
            {
                get
                {
                    for (int i = 0; i < masterTable.Length; i++)
                    {
                        GcObject[] subTable = masterTable[i];
                        if (subTable == null)
                        {
                            continue;
                        }

                        for (int j = 0; j < subTable.Length; j++)
                        {
                            for (GcObject gcObject = subTable[j]; gcObject != null; gcObject = gcObject.nextInHash)
                            {
                                yield return gcObject;
                            }
                        }
                    }
                }
            }
        }
    }
}
