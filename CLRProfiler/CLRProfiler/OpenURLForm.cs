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
    /// Summary description for AttachTargetPIDForm.
    /// </summary>

    public partial class OpenURLForm : System.Windows.Forms.Form
    {

        public OpenURLForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public string GetURL()
        {
            return URLtextBox.Text;
        }

    }
}
