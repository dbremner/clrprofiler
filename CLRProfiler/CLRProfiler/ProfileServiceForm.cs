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
    /// Summary description for ProfileServiceForm.
    /// </summary>
    public sealed partial class ProfileServiceForm : System.Windows.Forms.Form
    {

        public ProfileServiceForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        private void serviceNameTextBox_TextChanged(object sender, System.EventArgs e)
        {
            startCommandTextBox.Text = "net start " + serviceNameTextBox.Text;
            stopCommandTextBox .Text = "net stop "  + serviceNameTextBox.Text;
        }
    }
}
