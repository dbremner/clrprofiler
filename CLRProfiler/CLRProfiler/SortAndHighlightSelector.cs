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

        internal SortAndHighlightSelector(CallTreeForm.SortingBehaviour sort, CallTreeForm.SortingBehaviour highlight)
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
            sortCounter.SelectedIndex = 1 + sort.counterId;
            sortOrder.SelectedIndex = (1 + sort.sortingOrder) / 2;

            for(i = 0; i < numCounters; i++)
            {
                highlightCounter.Items.Add(Statistics.GetCounterName(i).ToLower());
            }
            highlightCounter.SelectedIndex = highlight.counterId;
            highlightOrder.SelectedIndex = (1 + highlight.sortingOrder) / 2;
        }

        internal void GetSortResults([NotNull] CallTreeForm.SortingBehaviour s,
                                     [NotNull] CallTreeForm.SortingBehaviour h)
        {
            s.counterId = sortCounter.SelectedIndex - 1;
            s.sortingOrder = sortOrder.SelectedIndex * 2 - 1;

            h.counterId = highlightCounter.SelectedIndex;
            h.sortingOrder = highlightOrder.SelectedIndex * 2 - 1;
        }
    }
}
