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
    /// Summary description for ViewCommentsForm.
    /// </summary>
    public partial class ViewCommentsForm : System.Windows.Forms.Form
    {

        internal ViewCommentsForm(ReadNewLog log)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            string[] lines = new string[log.commentEventList.count];
            for (int i = 0; i < log.commentEventList.count; i++)
            {
                lines[i] = string.Format("{0} ({1:f3} secs)", log.commentEventList.eventString[i], log.TickIndexToTime(log.commentEventList.eventTickIndex[i]));
            }

            this.commentTextBox.Lines = lines;
        }
    }
}
