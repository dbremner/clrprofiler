using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal class DiffStatistics
    {
        private readonly DiffDataNode node;
        private readonly static string[] CounterNames =
        {
            "Prev (incl)",
            "curr (incl)",
            "diff (incl)",
            "prev (Calls)",
            "curr (Calls)",
            "diff (Calls)"
        };
        internal readonly static int[] DefaultCounters = {0, 1, 2, 3, 4, 5};

        internal static bool IsInclusive(int id)
        {
            return CounterNames[id].EndsWith("(incl)");
        }

        internal long GetCounterValue(int id)
        {
            switch(id)
            {
                case 0:  return node.prevIncl;
                case 1:  return node.currIncl;
                case 2:  return node.currIncl - node.prevIncl;
                case 3:  return node.prevCalls;
                case 4:  return node.currCalls;
                case 5:  return node.currCalls - node.prevCalls;
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

        internal readonly bool firstTimeBroughtIn;
		
        internal DiffStatistics([NotNull] DiffDataNode node)
        {
            this.node = node;
			
            firstTimeBroughtIn = false;
        }

		
    }
}