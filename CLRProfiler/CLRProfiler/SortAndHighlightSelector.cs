/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  SortAndHighlightSelector
 *
 * Description: Form for selecting sorting options
 */

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for SortAndHighlightSelector.
    /// </summary>
    internal partial class SortAndHighlightSelector : System.Windows.Forms.Form
    {

        internal SortAndHighlightSelector(SortingBehaviour sort, SortingBehaviour highlight)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //
            // TODO: Add any constructor code after InitializeComponent call
            //
            int i, numCounters = Statistics.GetNumberOfCounters();
            sortCounter.Items.Add("in order of execution");
            for(i = 0; i < numCounters; i++)
            {
                sortCounter.Items.Add("by " + Statistics.GetCounterName(i).ToLower());
            }
            sortCounter.SelectedIndex = 1 + sort.CounterId;
            sortOrder.SelectedIndex = (1 + sort.SortingOrder) / 2;

            for(i = 0; i < numCounters; i++)
            {
                highlightCounter.Items.Add(Statistics.GetCounterName(i).ToLower());
            }
            highlightCounter.SelectedIndex = highlight.CounterId;
            highlightOrder.SelectedIndex = (1 + highlight.SortingOrder) / 2;
        }

        internal void GetSortResults([NotNull] SortingBehaviour s,
                                     [NotNull] SortingBehaviour h)
        {
            s.CounterId = sortCounter.SelectedIndex - 1;
            s.SortingOrder = sortOrder.SelectedIndex * 2 - 1;

            h.CounterId = highlightCounter.SelectedIndex;
            h.SortingOrder = highlightOrder.SelectedIndex * 2 - 1;
        }
    }
}
