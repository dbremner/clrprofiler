/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  ViewFilter
 *
 * Description: Dialog box for selecting which events are displayed
 *              in the call tree view
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for ViewFilter.
    /// </summary>
    public sealed partial class ViewFilter : System.Windows.Forms.Form
    {

        public ViewFilter(bool in_calls, bool in_allocs, bool in_assemblies)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            callsCheckbox.Checked = in_calls;
            allocationsCheckbox.Checked = in_allocs;
            assembliesCheckbox.Checked = in_assemblies;
        }
    }
}
