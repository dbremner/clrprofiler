using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal class Statistics
    {
        private readonly static string[] CounterNames =
        {
            "Calls (incl)",
            "Calls",
            "Bytes (incl)",
            "Bytes",
            "Objects (incl)",
            "Objects",
            "New functions (incl)",
            "Unmanaged calls (incl)",
            "Unmanaged calls",
            "Assemblies loaded (incl)",
            "Assemblies loaded"
        };

        internal readonly static int[] DefaultCounters = {0, 2, 4, 6, 9};

        internal static bool IsInclusive(int id)
        {
            return CounterNames[id].EndsWith("(incl)");
        }

        internal long GetCounterValue(int id)
        {
            switch(id)
            {
                case 0:  return this.numberOfFunctionsCalled;
                case 1:  return this.numberOfFunctionsCalled - this.numberOfFunctionsCalledByKids;
                case 2:  return this.bytesAllocated;
                case 3:  return this.bytesAllocated - this.bytesAllocatedByKids;
                case 4:  return this.numberOfObjectsAllocated;
                case 5:  return this.numberOfObjectsAllocated - this.numberOfObjectsAllocatedByKids;
                case 6:  return this.numberOfNewFunctionsBroughtIn;
                case 7:  return this.numberOfUnmanagedTransitions;
                case 8:  return this.numberOfUnmanagedTransitions - this.numberOfUnmanagedTransitionsByKids;
                case 9:  return this.numberOfAssembliesLoaded;
                case 10: return this.numberOfAssembliesLoaded - this.numberOfAssembliesLoadedByKids;

                default:
                {
                    return -1;
                }
            }
        }

        internal static int GetNumberOfCounters()
        {
            return CounterNames.Length;
        }

        internal static string GetCounterName(int id)
        {
            return CounterNames[id];
        }

        /* made internal since they are fairly independent
         * and creating accessors wouldn't gain anything */
        internal long bytesAllocated;
        internal long bytesAllocatedByKids;
        internal long numberOfObjectsAllocated;
        internal long numberOfObjectsAllocatedByKids;
        internal long numberOfFunctionsCalled;
        internal long numberOfFunctionsCalledByKids;
        internal long numberOfNewFunctionsBroughtIn;
        internal long numberOfUnmanagedTransitions;
        internal long numberOfUnmanagedTransitionsByKids;
        internal long numberOfAssembliesLoaded;
        internal long numberOfAssembliesLoadedByKids;

        internal bool firstTimeBroughtIn;
        
        internal Statistics()
        {
            bytesAllocated = 0;
            bytesAllocatedByKids = 0;
            numberOfObjectsAllocated = 0;
            numberOfObjectsAllocatedByKids = 0;
            numberOfFunctionsCalled = 0;
            numberOfFunctionsCalledByKids = 0;
            numberOfNewFunctionsBroughtIn = 0;
            numberOfUnmanagedTransitions = 0;
            numberOfUnmanagedTransitionsByKids = 0;
            numberOfAssembliesLoaded = 0;
            numberOfAssembliesLoadedByKids = 0;

            firstTimeBroughtIn = false;
        }

        /* write the contents to the backing store */
        internal void Write(BitWriter bw)
        {
            Helpers.WriteNumber(bw, bytesAllocated);
            Helpers.WriteNumber(bw, bytesAllocatedByKids);
            Helpers.WriteNumber(bw, numberOfObjectsAllocated);
            Helpers.WriteNumber(bw, numberOfObjectsAllocatedByKids);
            Helpers.WriteNumber(bw, numberOfFunctionsCalled);
            Helpers.WriteNumber(bw, numberOfFunctionsCalledByKids);
            Helpers.WriteNumber(bw, numberOfNewFunctionsBroughtIn);
            Helpers.WriteNumber(bw, numberOfUnmanagedTransitions);
            Helpers.WriteNumber(bw, numberOfUnmanagedTransitionsByKids);
            Helpers.WriteNumber(bw, numberOfAssembliesLoaded);
            Helpers.WriteNumber(bw, numberOfAssembliesLoadedByKids);
            bw.WriteBits(firstTimeBroughtIn ? 1ul : 0ul, 1);
        }

        /* read the contents to the backing store */
        internal void Read(BitReader br)
        {
            bytesAllocated = Helpers.ReadNumber(br);
            bytesAllocatedByKids = Helpers.ReadNumber(br);
            numberOfObjectsAllocated = Helpers.ReadNumber(br);
            numberOfObjectsAllocatedByKids = Helpers.ReadNumber(br);
            numberOfFunctionsCalled = Helpers.ReadNumber(br);
            numberOfFunctionsCalledByKids = Helpers.ReadNumber(br);
            numberOfNewFunctionsBroughtIn = Helpers.ReadNumber(br);
            numberOfUnmanagedTransitions = Helpers.ReadNumber(br);
            numberOfUnmanagedTransitionsByKids = Helpers.ReadNumber(br);
            numberOfAssembliesLoaded = Helpers.ReadNumber(br);
            numberOfAssembliesLoadedByKids = Helpers.ReadNumber(br);
            firstTimeBroughtIn = (br.ReadBits(1) != 0);
        }

        /* initialize from the backing store */
        internal Statistics(BitReader br)
        {
            Read(br);
        }
    }
}