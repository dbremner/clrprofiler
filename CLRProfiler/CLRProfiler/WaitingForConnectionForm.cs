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
    /// Summary description for WaitingForConnectionn.
    /// </summary>
    public partial class WaitingForConnectionForm : System.Windows.Forms.Form
    {

        public WaitingForConnectionForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        public void addMessage(string message)
        {
            messageTextBox.Text += "\n" + message;
        }

        public void setMessage(string message)
        {
            messageTextBox.Text = message;
        }
    }
}
