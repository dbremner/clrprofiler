// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for CommentRangeForm.
    /// </summary>
    public sealed partial class CommentRangeForm : System.Windows.Forms.Form
    {

        internal const string startCommentString = "Start of Application";
        internal const string shutdownCommentString = "Shutdown of Application";

        private void FillComboBoxes()
        {
            ReadNewLog log = MainForm.instance.log;

            startComboBox.Items.Add(startCommentString);
            endComboBox.Items.Add(startCommentString);

            for (int i = 0; i < log.commentEventList.count; i++)
            {
                string comment = log.commentEventList.eventString[i];
                startComboBox.Items.Add(comment);
                endComboBox.Items.Add(comment);
            }

            startComboBox.Items.Add(shutdownCommentString);
            endComboBox.Items.Add(shutdownCommentString);

            startComboBox.SelectedIndex = 0;
            endComboBox.SelectedIndex = endComboBox.Items.Count - 1;
            if (startComboBox.Items.Count > 2)
            {
                startComboBox.SelectedIndex = 1;
            }

            if (endComboBox.Items.Count > 2)
            {
                endComboBox.SelectedIndex = 2;
            }
        }

        internal CommentRangeForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            FillComboBoxes();
        }

        internal int startTickIndex;
        internal int endTickIndex;
        internal string startComment;
        internal string endComment;

        private int GetTickIndexOfSelectedIndex(int selectedIndex)
        {
            ReadNewLog log = MainForm.instance.log;
            if (selectedIndex == 0)
            {
                return 0;
            }
            else if (selectedIndex - 1 < log.commentEventList.count)
            {
                return log.commentEventList.eventTickIndex[selectedIndex - 1];
            }
            else
            {
                return log.maxTickIndex;
            }
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            int selectedIndex = startComboBox.SelectedIndex;
            startComment = (string)startComboBox.Items[selectedIndex];
            startTickIndex = GetTickIndexOfSelectedIndex(selectedIndex);
            selectedIndex = endComboBox.SelectedIndex;
            endComment = (string)endComboBox.Items[selectedIndex];
            endTickIndex = GetTickIndexOfSelectedIndex(selectedIndex);
        }
    }
}
