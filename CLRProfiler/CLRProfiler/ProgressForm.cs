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
    /// Summary description for Form2.
    /// </summary>
    public sealed partial class ProgressForm : System.Windows.Forms.Form
    {

        public ProgressForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            progressBar.Minimum = 0;
            progressBar.Maximum = 100;
        }

        public void setProgress(int value)
        {
            progressBar.Value = value;
        }

        public void setMaximum(int value)
        {
            progressBar.Maximum = value;
        }
    }
}
