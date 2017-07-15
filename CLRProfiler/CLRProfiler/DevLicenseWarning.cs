﻿// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CLRProfiler
{
    public partial class DevLicenseWarningForm : Form
    {
        public bool ShowThis { get; private set; } = true;

        public DevLicenseWarningForm()
        {
            InitializeComponent();
            AcceptButton = buttonOK;
        }

        private void showThisCheckbox_CheckedChanged(object sender, EventArgs e)
        {
            ShowThis = showThisCheckbox.Checked;
        }
    }
}
