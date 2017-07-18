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
        ArrayList ProcessNodes([NotNull] ArrayList nodesAtOneLevel);

        Font GetFont([NotNull] TreeNodeBase node);

        Color GetColor([NotNull] TreeNodeBase node, bool positive);

        object GetInfo([NotNull] TreeNodeBase node, [CanBeNull] ColumnInformation info);

        [NotNull]
        ArrayList FetchKids([NotNull] TreeNodeBase node);
		
    }
}