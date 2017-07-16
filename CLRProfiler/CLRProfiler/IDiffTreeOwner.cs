using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal interface IDiffTreeOwner
    {
        [NotNull]
        ArrayList ProcessNodes(object obj, [NotNull] ArrayList nodesAtOneLevel);

        Font GetFont(object obj, [NotNull] TreeNodeBase node);

        Color GetColor(object obj, [NotNull] TreeNodeBase node, bool positive);

        object GetInfo(object obj, [NotNull] TreeNodeBase node, [CanBeNull] ColumnInformation info);

        [NotNull]
        ArrayList FetchKids(object obj, [NotNull] TreeNodeBase node);
		
    }
}