/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  ListViewer
 *
 * Description: Form for displaying function/object lists (invoked
 *              from call tree view)
 */

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for ListViewer.
    /// </summary>
    public partial class ListViewer : System.Windows.Forms.Form, IComparer
    {

        private int sortColumn, sorting;

        public ListViewer()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            sortColumn = 0;
            sorting = 0;

            list.ListViewItemSorter = this;

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void list_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            if(e.Column == sortColumn)
            {
                sorting *= -1;
            }
            else
            {
                sortColumn = e.Column;
                sorting = (sortColumn == 0 ? 1 : -1);
            }
            list.Sort();
        }
        #region IComparer Members

        public int Compare(object x, object y)
        {
            int res;
            var a = (ListViewItem) x;
            var b = (ListViewItem) y;
            string aa = a.SubItems[sortColumn].Text, bb = b.SubItems[sortColumn].Text;
            if(sortColumn != 0)
            {
                long a1 = Int64.Parse(aa);
                long b1 = Int64.Parse(bb);
                res = (a1 > b1 ? 1 : (a1 == b1 ? 0 : -1));
            }
            else
            {
                res = aa.CompareTo(bb);
            }
            return sorting * res;
        }

        #endregion

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            var sf = new SaveFileDialog();
            sf.Filter = "Formatted text|*.txt|Comma-Separated Values|*.csv";
            DialogResult r = sf.ShowDialog();
            if(r == DialogResult.OK)
            {
                try
                {
                    if(File.Exists(sf.FileName))
                    {
                        File.Delete(sf.FileName);
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Cannot delete existing file " + sf.FileName, "Failure");
                    return;
                }

                bool formatNicely = !sf.FileName.ToLower().EndsWith(".csv");

                int columns = list.Columns.Count;
                try
                {
                    var s = new StreamWriter(sf.FileName);
                    string[] formats = new string[columns];
                    int i;
                    int j;
                    if(formatNicely)
                    {
                        // figure out widths of the columns
                        int[] widths = new int[columns];
                        for(i = 0; i < columns; i++)
                        {
                            widths[i] = list.Columns[i].Text.Length;
                        }
                        for(i = 0; i < list.Items.Count; i++)
                        {
                            ListViewItem m = list.Items[i];
                            for(j = 0; j < columns; j++)
                            {
                                int l = m.SubItems[j].Text.Length;
                                if(l > widths[j])
                                {
                                    widths[j] = l;
                                }
                            }
                        }

                        // create formats
                        for(i = 0; i < columns; i++)
                        {
                            formats[i] = "{0," + (i == 0 ? "-" : "") + widths[i] + '}' + (i == columns - 1 ? '\n' : ' ');
                        }
                    }
                    else
                    {
                        for(i = 0; i < columns - 1; i++)
                        {
                            formats[i] = "{0},";
                        }
                        formats[columns - 1] = "{0}\n";
                    }

                    for(i = 0; i < columns; i++)
                    {
                        s.Write(formats[i], list.Columns[i].Text);
                    }
                    if(formatNicely)
                    {
                        s.Write('\n');
                    }
                    for(i = 0; i < list.Items.Count; i++)
                    {
                        ListViewItem m = list.Items[i];
                        for(j = 0; j < columns; j++)
                        {
                            s.Write(formats[j], m.SubItems[j].Text);
                        }
                    }
                    s.Close();
                }
                catch
                {
                    MessageBox.Show(this, "Error saving table to a file named " + sf.FileName, "Failure");
                    return;
                }
            }
        }
    }
}
