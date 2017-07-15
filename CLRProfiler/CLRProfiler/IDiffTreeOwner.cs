using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal interface IDiffTreeOwner
    {
        ArrayList ProcessNodes(object obj, ArrayList nodesAtOneLevel);

        Font GetFont(object obj, TreeNodeBase node);

        Color GetColor(object obj, TreeNodeBase node, bool positive);

        object GetInfo(object obj, TreeNodeBase node, ColumnInformation info);

        ArrayList FetchKids(object obj, TreeNodeBase node);
		
    }
}