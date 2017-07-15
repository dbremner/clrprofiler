using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal interface ITreeOwner
    {
        ArrayList ProcessNodes(object obj, ArrayList nodesAtOneLevel);

        Font GetFont(object obj, TreeNodeBase node);

        Color GetColor(object obj, TreeNodeBase node, bool positive);

        object GetInfo(object obj, TreeNodeBase node, ColumnInformation info);

        ArrayList FetchKids(object obj, TreeNodeBase node);

        string MakeNameForFunction(int functionId);
        string MakeNameForAllocation(int typeId, int bytes);

        CallTreeForm.FnViewFilter[] GetIncludeFilters();
        CallTreeForm.FnViewFilter[] GetExcludeFilters();

        void SetIncludeFilters( CallTreeForm.FnViewFilter[] filters);
        void SetExcludeFilters( CallTreeForm.FnViewFilter[] filters);

        int GetNodeId( TreeNodeBase node );

        void RegenerateTree();
    }
}