// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System.Collections.Generic;

namespace CLRProfiler
{
    public partial class ObjectGraph
    {
        // this is optimized for space at the expense of programming convenience
        // heap dumps can contain many millions of objects
        internal class GcObject
        {
            internal GcObject parent;
            internal Vertex vertex;
            internal int idTypeSizeStackTraceId;
            const int idBits = IdToObject.idBits;
            const int idMask = IdToObject.idMask;
            const int typeSizeStackTraceIdBits = 32 - idBits;
            internal int Id
            {
                get
                {
                    return idTypeSizeStackTraceId & idMask;
                }
                set
                {
                    idTypeSizeStackTraceId = (idTypeSizeStackTraceId & ~idMask) | (value & idMask);
                }
            }
            internal int TypeSizeStackTraceId
            {
                get
                {
                    return idTypeSizeStackTraceId >> idBits;
                }
                set
                {
                    idTypeSizeStackTraceId = (idTypeSizeStackTraceId & idMask) | (value << idBits);
                }
            }
            internal uint Size(ObjectGraph graph)
            {
                int typeSizeStackTraceId = TypeSizeStackTraceId;
                if (typeSizeStackTraceId < 0)
                {
                    return 0;
                }

                int[] stackTrace = graph.readNewLog.stacktraceTable.IndexToStacktrace(typeSizeStackTraceId);
                return (uint)stackTrace[1];
            }

            internal GcType Type(ObjectGraph graph)
            {
                int typeSizeStackTraceId = TypeSizeStackTraceId;
                if (typeSizeStackTraceId < 0)
                {
                    return graph.typeIdToGcType[typeSizeStackTraceId];
                }
                else
                {
                    int[] stackTrace = graph.readNewLog.stacktraceTable.IndexToStacktrace(typeSizeStackTraceId);
                    int typeID = stackTrace[0];
                    return graph.typeIdToGcType[typeID];
                }
            }

            internal int interestLevelAllocTickIndex; // high 8 bits for interest level, low 24 bits for allocTickIndex;
            internal InterestLevel InterestLevel
            {
                get
                {
                    return (InterestLevel)(interestLevelAllocTickIndex >> 24);
                }
                set
                {
                    interestLevelAllocTickIndex &= 0x00ffffff;
                    interestLevelAllocTickIndex |= (int)value << 24;
                }
            }
            internal int AllocTickIndex
            {
                get
                {
                    return interestLevelAllocTickIndex & 0x00ffffff;
                }
                set
                {
                    interestLevelAllocTickIndex &= unchecked((int)0xff000000);
                    interestLevelAllocTickIndex |= value & 0x00ffffff;
                }
            }
            internal virtual IEnumerable<GcObject> References
            {
                get
                {
                    yield break;
                }
            }
            internal virtual void SetReference(int referenceNumber, GcObject target)
            {
            }
            internal GcObject nextInHash;

            internal static GcObject CreateGcObject(int numberOfReferences)
            {
                switch (numberOfReferences)
                {
                    case 0: return new GcObject();
                    case 1: return new GcObjectWith1Reference();
                    case 2: return new GcObjectWith2References();
                    case 3: return new GcObjectWith3References();
                    case 4: return new GcObjectWith4References();
                    case 5: return new GcObjectWith5References();
                    default: return new GcObjectWithManyReferences(numberOfReferences);
                }
            }
        }
    }
}
