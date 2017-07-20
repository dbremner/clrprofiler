using System.Collections;
using JetBrains.Annotations;

namespace CLRProfiler
{
    public sealed partial class AllocationDiff 
	{
	    private sealed class CompareIncl : IComparer
		{
		    [NotNull] private readonly Hashtable inclOfNode;

			internal CompareIncl([NotNull] Hashtable inclOfNode)
			{
				this.inclOfNode = inclOfNode;
			}

			int IComparer.Compare(object x, object y)
			{
				long inclX = (long)inclOfNode[x];
				long inclY = (long)inclOfNode[y];
				if (inclX < inclY)
                {
                    return 1;
                }
                else if (inclX > inclY)
                {
                    return -1;
                }
                else
                {
                    return 0;
                }
            }
		}
	}
}