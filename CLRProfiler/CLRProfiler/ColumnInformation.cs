using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal class ColumnInformation
    {
        internal enum ColumnTypes {Tree, String, OwnerDraw};

        internal readonly string Text;
        internal readonly int Token;
        internal readonly ColumnTypes ColumnType;

        internal ColumnInformation(int token, string name, ColumnTypes ct)
        {
            Text = name;
            Token = token;
            ColumnType = ct;
        }
    }
}