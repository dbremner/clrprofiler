/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  SelectColumns
 *
 * Description: Dialog box for selecting columns displayed in the
 *              call tree view
 */

using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for SelectColumns.
    /// </summary>
    public partial class SelectColumns : System.Windows.Forms.Form
    {

        private readonly List<CheckBox> checkBoxes;

        public void Set(int id)
        {
            checkBoxes[id].Checked = true;
        }

        public List<int> GetCheckedColumns()
        {
            var r = new List<int>();
            for(int i = 0; i < checkBoxes.Count; i++)
            {
                if(checkBoxes[i].Checked)
                {
                    r.Add(i);
                }
            }
            r.Sort();
            return r;
        }

        public SelectColumns()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //

            checkBoxes = new List<CheckBox>();

            var underscored = new HashSet<char>();

            int numCounters = Statistics.GetNumberOfCounters();
            for(int i = 0; i < numCounters; i++)
            {
                string text = Statistics.GetCounterName(i);
                for(int j = 0; j < text.Length; j++)
                {
                    if(!Char.IsLetter(text, j))
                    {
                        continue;
                    }

                    char c = Char.ToLower(text[j]);
                    if(!underscored.Contains(c))
                    {
                        underscored.Add(c);
                        text = text.Substring(0, j) + "&" + text.Substring(j);
                        break;
                    }
                }

                int px = (i % 2) == 1 ? Width / 2 : 16, py = 56;
                if(i > 1)
                {
                    py = checkBoxes[i - 2].Bottom;
                }

                CheckBox box = new CheckBox();
                box.Parent = this;
                box.Visible = true;
                box.Location = new Point(px, py);
                box.Width = Width / 2 - 10;
                box.Text = text;
                box.BringToFront();

                checkBoxes.Add(box);
            }
        }
    }
}
