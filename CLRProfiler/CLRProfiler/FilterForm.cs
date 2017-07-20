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
using System.Globalization;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for FilterForm.
    /// </summary>
    public sealed partial class FilterForm : System.Windows.Forms.Form
    {

        private string[] typeFilters = new string[0];
        internal string[] methodFilters = new string[0];
        internal string[] signatureFilters = new string[0];
        internal ulong[] addressFilters = new ulong[0];
        private bool showChildren = true;
        private bool showParents = true;
        private bool caseInsensitive = true;
        private bool onlyFinalizableTypes = false;
        internal int filterVersion;
        private static int versionCounter;


        // Given the set of names: [aaa, aab, aac, aba, abb, abc, aca, acb, acc],
        // The filters are applied sequentially and behave in the following manner:
        //
        // "aa"       => 'aa*'                    => [aaa, aab, aac]
        // "~aa"      => not 'aa*'                => [aba, abb, abc, aca, acb, acc]
        // "aa;ab"    => 'aa*' or 'ab*'           => [aaa, aab, aac, aba, abb, abc]
        // "aa;~aab"  => 'aa*' and not 'aab*'     => [aaa, aac]
        // "~aa;aaa"  => not 'aa*' or 'aaa*'      => [aaa, aba, abb, abc, aca, acb, acc]
        // "~aa;~abb" => not 'aa*' and not 'abb*' => [aba, abc, aca, acb, acc]
        //
        internal bool IsInterestingName(string name, string[] typeFilters)
        {
            if (name == "<root>" || typeFilters.Length == 0)
            {
                return true;
            }

            bool? isInteresting = null;
            foreach (string filter in typeFilters)
            {
                bool isInclusiveFilter = true;
                string realFilter = filter.Trim();
                if (realFilter.Length > 0 && (realFilter[0] == '~' || realFilter[0] == '!'))
                {
                    isInclusiveFilter = false;
                    realFilter = realFilter.Substring(1).Trim();
                }

                // Skip empty filter, which handles accidental leading, trailing, or doubled semi-colons in the filter string
                if (realFilter.Length == 0)
                {
                    continue;
                }

                if (isInteresting == null || (isInteresting.Value ^ isInclusiveFilter))
                {
                    bool isPrefixMatch = string.Compare(name, 0, realFilter, 0, realFilter.Length, caseInsensitive, CultureInfo.InvariantCulture) == 0;
                    if (isPrefixMatch)
                    {
                        isInteresting = isInclusiveFilter;
                    }
                    else if (isInteresting == null)
                    {
                        isInteresting = !isInclusiveFilter;
                    }
                }
            }
            return isInteresting ?? true;
        }

        internal bool IsInterestingAddress(ulong thisAddress)
        {
            if (thisAddress == 0 || addressFilters.Length == 0)
            {
                return true;
            }

            foreach (ulong address in addressFilters)
            {
                if (address == thisAddress)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsInterestingSignature(string signature, string[] signatureFilters)
        {
            if (signature == null || signature == "" || signatureFilters.Length == 0)
            {
                return true;
            }

            return IsInterestingName(signature, signatureFilters);
        }

        internal bool IsInterestingTypeName(string typeName, string signature, bool typeIsFinalizable)
        {
            if (onlyFinalizableTypes && !typeIsFinalizable && typeName != "<root>")
            {
                return false;
            }

            return IsInterestingName(typeName, typeFilters) && IsInterestingSignature(signature, signatureFilters);
        }

        internal bool IsInterestingMethodName(string methodName, string signature)
        {
            return IsInterestingName(methodName, methodFilters) && IsInterestingSignature(signature, signatureFilters);
        }

        internal InterestLevel InterestLevelForParentsAndChildren()
        {
            InterestLevel interestLevel = InterestLevel.Ignore;
            if (showParents)
            {
                interestLevel |= InterestLevel.Parents;
            }

            if (showChildren)
            {
                interestLevel |= InterestLevel.Children;
            }

            return interestLevel;
        }

        public FilterForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        internal void SetFilterForm(string typeFilter, string methodFilter, string signatureFilter, string addressFilter,
            bool showAncestors, bool showDescendants, bool caseInsensitive, bool onlyFinalizableTypes)
        {
            typeFilter = typeFilter.Trim();
            if (typeFilter == "")
            {
                typeFilters = new string[0];
            }
            else
            {
                typeFilters = typeFilter.Split(';');
            }

            methodFilter = methodFilter.Trim();
            if (methodFilter == "")
            {
                methodFilters = new string[0];
            }
            else
            {
                methodFilters = methodFilter.Split(';');
            }

            signatureFilter = signatureFilter.Trim();
            if (signatureFilter == "")
            {
                signatureFilters = new string[0];
            }
            else
            {
                signatureFilters = signatureFilter.Split(';');
            }

            addressFilter = addressFilter.Trim();
            if (addressFilter == "")
            {
                addressFilters = new ulong[0];
            }
            else
            {
                string[] addressFilterStrings = addressFilter.Split(';');
                addressFilters = new ulong[addressFilterStrings.Length];
                for (int i = 0; i < addressFilterStrings.Length; i++)
                {
                    string thisAddressFilter = addressFilterStrings[i].Replace(".", "");
                    if (thisAddressFilter != "")
                    {
                        if (thisAddressFilter.StartsWith("0x") || thisAddressFilter.StartsWith("0X"))
                        {
                            addressFilters[i] = ulong.Parse(thisAddressFilter.Substring(2), NumberStyles.HexNumber);
                        }
                        else
                        {
                            addressFilters[i] = ulong.Parse(thisAddressFilter, NumberStyles.HexNumber);
                        }
                    }
                }
            }
            this.showParents = showAncestors;
            this.showChildren = showDescendants;
            this.caseInsensitive = caseInsensitive;
            this.onlyFinalizableTypes = onlyFinalizableTypes;

            this.filterVersion = ++versionCounter;

            typeFilterTextBox.Text = typeFilter;
            methodFilterTextBox.Text = methodFilter;
            signatureFilterTextBox.Text = signatureFilter;
            addressFilterTextBox.Text = addressFilter;

            parentsCheckBox.Checked = showParents;
            childrenCheckBox.Checked = showChildren;
            caseInsensitiveCheckBox.Checked = caseInsensitive;
            onlyFinalizableTypesCheckBox.Checked = onlyFinalizableTypes;

        }

        private void okButton_Click(object sender, System.EventArgs e)
        {
            SetFilterForm(typeFilterTextBox.Text, methodFilterTextBox.Text, signatureFilterTextBox.Text, addressFilterTextBox.Text,
                parentsCheckBox.Checked, childrenCheckBox.Checked, caseInsensitiveCheckBox.Checked, onlyFinalizableTypesCheckBox.Checked);
        }
    }
}
