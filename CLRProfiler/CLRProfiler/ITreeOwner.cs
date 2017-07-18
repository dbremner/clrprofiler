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
        ArrayList ProcessNodes([NotNull] ArrayList nodesAtOneLevel);

        Font GetFont([NotNull] TreeNodeBase node);

        Color GetColor([NotNull] TreeNodeBase node, bool positive);

        object GetInfo(object obj, [NotNull] TreeNodeBase node, [CanBeNull] ColumnInformation info);

        [NotNull]
        ArrayList FetchKids([NotNull] TreeNodeBase node);

        [CanBeNull]
        string MakeNameForFunction(int functionId);

        [CanBeNull]
        string MakeNameForAllocation(int typeId, int bytes);

        CallTreeForm.FnViewFilter[] GetIncludeFilters();
        CallTreeForm.FnViewFilter[] GetExcludeFilters();

        void SetIncludeFilters( CallTreeForm.FnViewFilter[] filters);
        void SetExcludeFilters( CallTreeForm.FnViewFilter[] filters);

        int GetNodeId([NotNull] TreeNodeBase node );

        void RegenerateTree();
    }
}