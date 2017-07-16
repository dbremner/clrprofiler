using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal class TreeNode : TreeNodeBase
    {
        /* not to be stored externally */
        internal bool isunmanaged;
        internal bool highlighted;

        /* stored */
        internal enum NodeType {Call = 0, Allocation, AssemblyLoad};

        internal readonly NodeType nodetype;
        internal readonly int stackid;
        internal int nameId;
        internal long nodeOffset;       // Offset of this node in the trace log

        internal long prevOffset, kidOffset;

        [NotNull] internal readonly Statistics data;

        internal TreeNode(NodeType in_nodetype, int in_stackid) : base()
        {
            highlighted = false;
            data = new Statistics();
            nodetype = in_nodetype;
            stackid = in_stackid;

            prevOffset = kidOffset = nodeOffset = -1;
        }

        internal void Write([NotNull] BitWriter writer)
        {
            writer.WriteBits((ulong)nodetype, 2);
            Helpers.WriteNumber(writer, stackid);
            if(nodetype == NodeType.AssemblyLoad)
            {
                Helpers.WriteNumber(writer, nameId);
            }
            Helpers.WriteNumber(writer, 1 + kidOffset);
            Helpers.WriteNumber(writer, 1 + prevOffset);
            Helpers.WriteNumber(writer, nodeOffset);
            data.Write(writer);
        }

        /* initialize from the backing store */
        internal TreeNode([NotNull] BitReader reader)
        {
            nodetype = (NodeType)reader.ReadBits(2);
            stackid = (int)Helpers.ReadNumber(reader);
            if(nodetype == NodeType.AssemblyLoad)
            {
                nameId = (int)Helpers.ReadNumber(reader);
            }
            kidOffset = Helpers.ReadNumber(reader) - 1;
            prevOffset = Helpers.ReadNumber(reader) - 1;
            nodeOffset = Helpers.ReadNumber(reader);
            data = new Statistics(reader);
        }
    };
}