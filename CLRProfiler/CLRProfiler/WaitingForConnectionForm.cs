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
using JetBrains.Annotations;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for WaitingForConnectionn.
    /// </summary>
    public sealed partial class WaitingForConnectionForm : System.Windows.Forms.Form
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

        public WaitingForConnectionForm([NotNull] string message)
            : this()
        {
            messageTextBox.Text = message;
        }

        public void addMessage(string message)
        {
            messageTextBox.Text += "\n" + message;
        }
    }
}
