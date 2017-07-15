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
    /// Summary description for Form5.
    /// </summary>
    public partial class FindRoutineForm : System.Windows.Forms.Form
    {

        public FindRoutineForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }

        private void cancelButton_Click(object sender, System.EventArgs e)
        {
            Close();
        }
    }
}
