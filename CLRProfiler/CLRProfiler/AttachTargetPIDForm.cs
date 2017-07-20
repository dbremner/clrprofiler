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
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for AttachTargetPIDForm.
    /// </summary>
    public sealed partial class AttachTargetPIDForm : System.Windows.Forms.Form
    {

        public AttachTargetPIDForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        public int GetPID()
        {
            int pid = 0;
            try
            {
                pid = Int32.Parse(PIDtextBox.Text);
                Process.GetProcessById(pid);
            }
            catch (Exception e)
            {
                MessageBox.Show( string.Format("The process ID ({0}) is not valid : {1} ", PIDtextBox.Text, e.Message) );
                pid = 0;
            }
            return pid;
        }

    }
}
