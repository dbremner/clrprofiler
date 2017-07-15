/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  WindowsStoreAppChooserForm
 *
 * Description: Lists WindowsStoreApp packages installed for the current user,
 * and allows the user to pick a package + app to profile.
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CLRProfiler
{
    public partial class WindowsStoreAppChooserForm : Form
    {
        private List<PackageInfo> packageInfos;

        public string SelectedAppUserModelId { get; private set; }

        public string SelectedPackageFullName { get; private set; }

        public string SelectedPackageTempDir { get; private set; }

        public string SelectedAcSidString { get; private set; }

        public string SelectedProcessFileName { get; private set; }

        public WindowsStoreAppChooserForm()
        {
            // Query OS for packages
            packageInfos = WindowsStoreAppHelperWrapper.GetPackagesForCurrentUser();
            
            // Initialize controls
            InitializeComponent();

            AcceptButton = buttonOK;
            CancelButton = buttonCancel;

            // Populate Package listbox with package info
            listBoxPackages.BeginUpdate();
            for (int i = 0; i < packageInfos.Count; i++)
            {
                listBoxPackages.Items.Add(packageInfos[i].fullName);
            }
            listBoxPackages.EndUpdate();
            listBoxPackages.SelectedIndex = 0;
        }


        private void listBoxPackages_SelectedIndexChanged(object sender, EventArgs e)
        {
            int iPackage = listBoxPackages.SelectedIndex;

            // Populate static text controls with details about the package
            textBoxLocation.Text = packageInfos[iPackage].installedLocation;
            labelArchitecture.Text = packageInfos[iPackage].architecture;
            labelIdlName.Text = packageInfos[iPackage].name;
            labelPublisher.Text = packageInfos[iPackage].publisher;
            labelVersion.Text = packageInfos[iPackage].version;

            // Populate App listbox with apps from package
            listBoxApps.BeginUpdate();
            listBoxApps.Items.Clear();
            for (int i = 0; i < packageInfos[iPackage].appInfoList.Count; i++)
            {
                listBoxApps.Items.Add(packageInfos[iPackage].appInfoList[i].exeName);
            }
            listBoxApps.EndUpdate();
            listBoxApps.SelectedIndex = 0;
            SelectedPackageFullName = packageInfos[iPackage].fullName;
            SelectedPackageTempDir = packageInfos[iPackage].tempDir;
            SelectedAcSidString = packageInfos[iPackage].acSid;
        }

        private void listboxApps_SelectedIndexChanged(object sender, EventArgs e)
        {
            SelectedAppUserModelId = packageInfos[listBoxPackages.SelectedIndex].appInfoList[listBoxApps.SelectedIndex].userModelId;
            SelectedProcessFileName = Path.Combine(
                packageInfos[listBoxPackages.SelectedIndex].installedLocation,
                packageInfos[listBoxPackages.SelectedIndex].appInfoList[listBoxApps.SelectedIndex].exeName);
        }
    }
}
