using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CLRProfiler
{
    // Implements the manual sorting of items by columns.
    internal sealed class ListViewItemComparer : IComparer 
    {
        private readonly int sortCol;
        private readonly bool fReverseSort;
        private readonly int subtreeline;

        internal ListViewItemComparer() 
        {
            sortCol = 0;
            fReverseSort = false;
            subtreeline = -1;
        }

        internal ListViewItemComparer(int column, bool fReverse, int subtreeline) 
        {
            sortCol = column;
            fReverseSort = fReverse;
            this.subtreeline = subtreeline;
        }


        public int Compare(object x, object y) 
        {
            int retval;

            if (((ListViewItem)x).Index <= subtreeline || subtreeline == -1)
            {
                // Don't sort items above the "subtree" line
                return 0;
            }

            if (sortCol == 0)
            {
                // function name column.  compare as text
                retval = String.Compare(((ListViewItem)x).SubItems[sortCol].Text, ((ListViewItem)y).SubItems[sortCol].Text);
            }
            else
            {
                // data column. compare a number
                int valX = 0, valY = 0;

                if (((ListViewItem)x).SubItems.Count > sortCol)
                {                                                              
                    valX = Convert.ToInt32(((ListViewItem)x).SubItems[sortCol].Text);
                }

                if (((ListViewItem)y).SubItems.Count > sortCol)
                {                                                              
                    valY = Convert.ToInt32(((ListViewItem)y).SubItems[sortCol].Text);
                }               

                if (  valX < valY)                  
                {
                    retval = -1;
                }
                else if (valX == valY)
                {
                    retval = 0;
                }
                else
                {
                    retval = 1;
                }
            }

            if (fReverseSort)
            {
                retval = 0 - retval;
            }

            return retval;
        }
    }
}