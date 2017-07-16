using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal interface ITreeOwner
    {
        [NotNull]
        ArrayList ProcessNodes(object obj, [NotNull] ArrayList nodesAtOneLevel);

        Font GetFont(object obj, [NotNull] TreeNodeBase node);

        Color GetColor(object obj, [NotNull] TreeNodeBase node, bool positive);

        object GetInfo(object obj, [NotNull] TreeNodeBase node, ColumnInformation info);

        [NotNull]
        ArrayList FetchKids(object obj, [NotNull] TreeNodeBase node);

        [NotNull]
        string MakeNameForFunction(int functionId);

        [NotNull]
        string MakeNameForAllocation(int typeId, int bytes);

        CallTreeForm.FnViewFilter[] GetIncludeFilters();
        CallTreeForm.FnViewFilter[] GetExcludeFilters();

        void SetIncludeFilters( CallTreeForm.FnViewFilter[] filters);
        void SetExcludeFilters( CallTreeForm.FnViewFilter[] filters);

        int GetNodeId([NotNull] TreeNodeBase node );

        void RegenerateTree();
    }
}