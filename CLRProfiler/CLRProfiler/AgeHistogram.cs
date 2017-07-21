// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using JetBrains.Annotations;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for AgeHistogram.
    /// </summary>
    public sealed partial class AgeHistogram : System.Windows.Forms.Form
    {
        private Font font;
        private bool useMarkers;

        internal AgeHistogram()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            toolTip = new ToolTip
            {
                Active = false,
                ShowAlways = true,
                AutomaticDelay = 70,
                ReshowDelay = 1
            };

            autoUpdate = true;
        
            MainForm form1 = MainForm.instance;
            if (form1.lastLogResult != null)
            {
                liveObjectTable = form1.lastLogResult.liveObjectTable;
            }

            font = form1.font;
            markersRadioButton.Enabled = form1.log.commentEventList.count > 0;
        }

        internal AgeHistogram(LiveObjectTable liveObjectTable, string title) : this()
        {
            this.liveObjectTable = liveObjectTable;
            this.Text = title;
            autoUpdate = false;
        }

        [CanBeNull] private LiveObjectTable liveObjectTable;

        private const int leftMargin = 30;
        private int bottomMargin = 50;
        private const int gap = 20;
        private int bucketWidth = 50;
        private const int topMargin = 30;
        private const int rightMargin = 30;
        private const int minHeight = 400;

        private double timeScale = 1;
        private uint verticalScale = 0;

        private double GetTimeScale(double suggestedScale, bool firstTime)
        {
            if (!firstTime)
            {
                // If a radio button is already checked, return its scale
                foreach (RadioButton rb in timeScaleGroupBox.Controls)
                {
                    if (rb.Checked)
                    {
                        if (rb == markersRadioButton)
                        {
                            return 0.001;
                        }
                        else
                        {
                            return Convert.ToDouble(rb.Text, CultureInfo.InvariantCulture);
                        }
                    }
                }
            }
            // Otherwise, try to find the lowest scale that is still at least as
            // large as the suggested scale. If there's no such thing, return the highest scale.
            double bestHigherScale = double.PositiveInfinity;
            RadioButton bestHigherRadioButton = null;
            double bestLowerScale = 0.0;
            RadioButton bestLowerRadioButton = null;

            foreach (RadioButton rb in timeScaleGroupBox.Controls)
            {
                if (rb == markersRadioButton)
                {
                    continue;
                }

                double scale = Convert.ToDouble(rb.Text, CultureInfo.InvariantCulture);
                if (scale >= suggestedScale)
                {
                    if (scale < bestHigherScale)
                    {
                        bestHigherScale = scale;
                        bestHigherRadioButton = rb;
                    }
                }
                else
                {
                    if (scale > bestLowerScale)
                    {
                        bestLowerScale = scale;
                        bestLowerRadioButton = rb;
                    }
                }
            }

            if (bestHigherRadioButton != null)
            {
                bestHigherRadioButton.Checked = true;
                return bestHigherScale;
            }
            else
            {
                Debug.Assert(bestLowerRadioButton != null);
                bestLowerRadioButton.Checked = true;
                return bestLowerScale;
            }
        }

        [CanBeNull] private TypeDesc[] typeIndexToTypeDesc;

        [CanBeNull] private Bucket[] bucketTable;

        [CanBeNull] private List<TypeDesc> sortedTypeTable;
        private ulong totalSize;

        private ulong BuildBuckets(double timeScale, double maxAge)
        {
            Debug.Assert(liveObjectTable != null, "liveObjectTable != null");
            ReadNewLog log = liveObjectTable.readNewLog;
            bool useMarkers = markersRadioButton.Checked;
            if (this.useMarkers != useMarkers)
            {
                this.useMarkers = useMarkers;
                bucketTable = null;
            }
            int bucketCount;
            if (useMarkers)
            {
                for (bucketCount = 0; bucketCount < log.commentEventList.count; bucketCount++)
                {
                    if (log.commentEventList.eventTickIndex[bucketCount] >= liveObjectTable.lastTickIndex)
                    {
                        break;
                    }
                }

                bucketCount++;
            }
            else
            {
                bucketCount = (int)Math.Ceiling(maxAge/timeScale);
                if (bucketCount == 0)
                {
                    bucketCount = 1;
                }
            }
            if (bucketTable == null || bucketTable.Length != bucketCount)
            {
                bucketTable = new Bucket[bucketCount];
                double nowTime = log.TickIndexToTime(liveObjectTable.lastTickIndex);
                double age = 0;
                for (int i = 0; i < bucketTable.Length; i++)
                {
                    bucketTable[i].totalSize = 0;
                    bucketTable[i].typeDescToSizeCount = new Dictionary<TypeDesc, SizeCount>();
                    bucketTable[i].minAge = age;
                    if (useMarkers)
                    {
                        int markerIndex = bucketTable.Length - i - 1;
                        if (i > 0)
                        {
                            bucketTable[i].minComment = log.commentEventList.eventString[markerIndex];
                        }

                        age = nowTime;
                        if (markerIndex > 0)
                        {
                            bucketTable[i].maxComment = log.commentEventList.eventString[markerIndex-1];
                            age -= log.TickIndexToTime(log.commentEventList.eventTickIndex[markerIndex-1]);                        
                        }
                    }
                    else
                    {
                        age += timeScale;
                    }
                    bucketTable[i].maxAge = age;
                }

                if (typeIndexToTypeDesc == null || typeIndexToTypeDesc.Length < log.typeName.Length)
                {
                    typeIndexToTypeDesc = new TypeDesc[log.typeName.Length];
                }
                else
                {
                    foreach (TypeDesc t in typeIndexToTypeDesc)
                    {
                        if (t != null)
                        {
                            t.totalSize = 0;
                        }
                    }
                }
                LiveObjectTable.LiveObject o;
                for (liveObjectTable.GetNextObject(0, ulong.MaxValue, out o); o.id < ulong.MaxValue; liveObjectTable.GetNextObject(o.id + o.size, ulong.MaxValue, out o))
                {
                    double allocTime = log.TickIndexToTime(o.allocTickIndex);
                    age = nowTime - allocTime;
                    int bucketIndex = (int)(age/timeScale);
                    if (bucketIndex >= bucketTable.Length)
                    {
                        bucketIndex = bucketTable.Length - 1;
                    }
                    else if (bucketIndex < 0)
                    {
                        bucketIndex = 0;
                    }

                    while (bucketIndex < bucketTable.Length - 1 && age >= bucketTable[bucketIndex].maxAge)
                    {
                        bucketIndex++;
                    }

                    while (bucketIndex > 0 && age < bucketTable[bucketIndex].minAge)
                    {
                        bucketIndex--;
                    }

                    bucketTable[bucketIndex].totalSize += o.size;
                    TypeDesc t = typeIndexToTypeDesc[o.typeIndex];
                    if (t == null)
                    {
                        t = new TypeDesc(log.typeName[o.typeIndex]);
                        typeIndexToTypeDesc[o.typeIndex] = t;
                    }
                    t.totalSize += o.size;
                    SizeCount sizeCount;
                    if (!bucketTable[bucketIndex].typeDescToSizeCount.TryGetValue(t, out sizeCount))
                    {
                        sizeCount = new SizeCount();
                        bucketTable[bucketIndex].typeDescToSizeCount[t] = sizeCount;
                    }
                    sizeCount.size += o.size;
                    sizeCount.count += 1;
                }
            }

            ulong maxBucketSize = 0;
            foreach (Bucket b in bucketTable)
            {
                if (maxBucketSize < b.totalSize)
                {
                    maxBucketSize = b.totalSize;
                }
            }

            totalSize = 0;
            sortedTypeTable = new List<TypeDesc>();
            Debug.Assert(typeIndexToTypeDesc != null);
            foreach (TypeDesc t in typeIndexToTypeDesc)
            {
                if (t != null)
                {
                    sortedTypeTable.Add(t);
                    totalSize += t.totalSize;
                }
            }

            sortedTypeTable.Sort();

            return maxBucketSize;
        }

        private uint GetScale([NotNull] GroupBox groupBox, int pixelsAvailable, ulong rangeNeeded, bool firstTime)
        {
            if (!firstTime)
            {
                foreach (RadioButton rb in groupBox.Controls)
                {
                    if (rb.Checked)
                    {
                        return UInt32.Parse(rb.Text);
                    }
                }
            }
            // No radio button was checked - let's come up with a suitable default
            RadioButton maxLowScaleRB = null;
            uint maxLowRange = 0;
            RadioButton minHighScaleRB = null;
            uint minHighRange = Int32.MaxValue;
            foreach (RadioButton rb in groupBox.Controls)
            {
                uint range = (uint)(pixelsAvailable*Int32.Parse(rb.Text));
                if (range < rangeNeeded)
                {
                    if (maxLowRange < range)
                    {
                        maxLowRange = range;
                        maxLowScaleRB = rb;
                    }
                }
                else
                {
                    if (minHighRange > range)
                    {
                        minHighRange = range;
                        minHighScaleRB = rb;
                    }
                }
            }
            if (minHighScaleRB != null)
            {
                minHighScaleRB.Checked = true;
                return UInt32.Parse(minHighScaleRB.Text);
            }
            else
            {
                Debug.Assert(maxLowScaleRB != null, "maxLowScaleRB != null");
                maxLowScaleRB.Checked = true;
                return UInt32.Parse(maxLowScaleRB.Text);
            }
        }

        private uint GetVerticalScale(int pixelsAvailable, ulong rangeNeeded, bool firstTime)
        {
            return GetScale(verticalScaleGroupBox, pixelsAvailable, rangeNeeded, firstTime);
        }

        private static readonly Color[] firstColors =
        {
            Color.Red,
            Color.Yellow,
            Color.Green,
            Color.Cyan,
            Color.Blue,
            Color.Magenta,
        };

        private static Color[] colors = new Color[16];

        private Color MixColor(Color a, Color b)
        {
            int R = (a.R + b.R)/2;
            int G = (a.G + b.G)/2;
            int B = (a.B + b.B)/2;

            return Color.FromArgb(R, G, B);
        }

        private static void GrowColors()
        {
            Color[] newColors = new Color[2*colors.Length];
            for (int i = 0; i < colors.Length; i++)
            {
                newColors[i] = colors[i];
            }

            colors = newColors;
        }

        [CanBeNull]
        private TypeDesc FindSelectedType()
        {
            Debug.Assert(sortedTypeTable != null, "sortedTypeTable != null");
            foreach (TypeDesc t in sortedTypeTable)
            {
                if (t.selected)
                {
                    return t;
                }
            }

            return null;
        }

        private void ColorTypes()
        {
            int count = 0;

            bool anyTypeSelected = FindSelectedType() != null;

            Debug.Assert(sortedTypeTable != null, "sortedTypeTable != null");
            foreach (TypeDesc t in sortedTypeTable)
            {
                if (count >= colors.Length)
                {
                    GrowColors();
                }

                if (count < firstColors.Length)
                {
                    colors[count] = firstColors[count];
                }
                else
                {
                    colors[count] = MixColor(colors[count - firstColors.Length], colors[count - firstColors.Length + 1]);
                }

                t.color = colors[count];
                if (anyTypeSelected)
                {
                    t.color = MixColor(colors[count], Color.White);
                }

                t.brush = new SolidBrush(t.color);
                count++;
            }
        }

        private string FormatSize(ulong size)
        {
            double w = size;
            string byteString = "bytes";
            if (w >= 1024)
            {
                w /= 1024;
                byteString = "kB";
            }
            if (w >= 1024)
            {
                w /= 1024;
                byteString = "MB";
            }
            if (w >= 1024)
            {
                w /= 1024;
                byteString = "GB";
            }
            string format = "{0:f0} {1}";
            if (w < 10)
            {
                format = "{0:f1} {1}";
            }

            return string.Format(format, w, byteString);
        }

        private string FormatTime(double time)
        {
            if (timeScale < 0.01)
            {
                return string.Format("{0:f3}", time);
            }
            else if (timeScale < 0.1)
            {
                return string.Format("{0:f2}", time);
            }
            else if (timeScale < 1.0)
            {
                return string.Format("{0:f1}", time);
            }
            else
            {
                return string.Format("{0:f0}", time);
            }
        }

        private void DrawBuckets(Graphics g)
        {
            Debug.Assert(verticalScale != 0);
            bool noBucketSelected = true;
            Debug.Assert(bucketTable != null, "bucketTable != null");
            foreach (Bucket b in bucketTable)
            {
                if (b.selected)
                {
                    noBucketSelected = false;
                    break;
                }
            }
            int x = leftMargin;
            Brush blackBrush = Brushes.Black;
            foreach (Bucket b in bucketTable)
            {
                int y = graphPanel.Height - bottomMargin;
                if (b.minComment != null || b.maxComment != null)
                {
                    string lessOrGreater = "<";
                    double age = b.maxAge;
                    string commentString = b.maxComment;
                    if (commentString == null)
                    {
                        lessOrGreater = ">";
                        age = b.minAge;
                        commentString = b.minComment;
                    }
                    string s = string.Format("{0} {1} sec", lessOrGreater, FormatTime(age));
                    g.DrawString(s, font, blackBrush, x, y);
                    s = commentString;
                    if (g.MeasureString(s, font).Width > bucketWidth)
                    {
                        do
                        {
                            s = s.Substring(0, s.Length-1);
                        }
                        while (g.MeasureString(s, font).Width > bucketWidth);
                        s += "...";
                    }
                    g.DrawString(s, font, blackBrush, x, y + font.Height);
                    s = FormatSize(b.totalSize);
                    g.DrawString(s, font, blackBrush, x, y + 2*font.Height);
                }
                else
                {
                    string s = string.Format("< {0} sec", FormatTime(b.maxAge));
                    g.DrawString(s, font, blackBrush, x, y);
                    s = FormatSize(b.totalSize);
                    g.DrawString(s, font, blackBrush, x, y + font.Height);
                }

                foreach (KeyValuePair<TypeDesc, SizeCount> d in b.typeDescToSizeCount)
                {
                    TypeDesc t = d.Key;
                    SizeCount sizeCount = d.Value;
                    int height = (int)(sizeCount.size/verticalScale);
                    y -= height;
                    bool cond = t.selected && (b.selected || noBucketSelected);
                    g.FillRectangle(cond ? blackBrush : t.brush, x, y, bucketWidth, height);
                }

                x += bucketWidth + gap;
            }
        }

        private int BucketWidth([NotNull] Graphics g)
        {
            int width1 = (int)g.MeasureString("< 999.999 sec", font).Width;
            int width2 = (int)g.MeasureString("999 MB", font).Width;
            width1 = Math.Max(width1, width2);
            if (markersRadioButton.Checked)
            {
                width1 = Math.Max(width1, (int)g.MeasureString("0123456789012345", font).Width);
            }

            return Math.Max(width1, bucketWidth);
        }

        private int BottomMargin()
        {
            return font.Height*3 + 10;
        }

        private ulong Init([NotNull] Graphics g)
        {
            bucketWidth = BucketWidth(g);
            bottomMargin = BottomMargin();
            Debug.Assert(liveObjectTable != null, "liveObjectTable != null");
            double maxAge = liveObjectTable.readNewLog.TickIndexToTime(liveObjectTable.lastTickIndex);
            int barsVisible = (graphOuterPanel.Width - leftMargin - rightMargin)/(bucketWidth + gap);

            timeScale = GetTimeScale(maxAge/barsVisible, timeScale == 0);

            ulong maxBucketSize = BuildBuckets(timeScale, maxAge);
            
            ColorTypes();

            return maxBucketSize;
        }

        private bool initialized;

        private void graphPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            initialized = false;

            if (liveObjectTable == null)
            {
                return;
            }

            Graphics g = e.Graphics;

            ulong maxBucketSize = Init(g);

            int pixelsForBars = graphOuterPanel.Height - topMargin - bottomMargin;

            verticalScale = GetVerticalScale(pixelsForBars, maxBucketSize/1024, verticalScale == 0)*1024;

            Debug.Assert(bucketTable != null, "bucketTable != null");
            int bucketCount = bucketTable.Length;
            int width = leftMargin + bucketWidth*bucketCount + gap*(bucketCount-1) + rightMargin;
            graphPanel.Width = width;

            int height = topMargin + (int)(maxBucketSize/verticalScale) + bottomMargin;
            graphPanel.Height = height;

            DrawBuckets(g);

            initialized = true;
        }

        private const int typeLegendSpacing = 3;
        private int dotSize = 8;

        private void DrawTypeLegend([NotNull] Graphics g)
        {
            dotSize = (int)g.MeasureString("0", font).Width;
            int maxWidth = 0;
            int x = leftMargin;
            int y = topMargin;
            Debug.Assert(sortedTypeTable != null, "sortedTypeTable != null");
            foreach (TypeDesc t in sortedTypeTable)
            {
                int typeNameWidth = (int)g.MeasureString(t.typeName, font).Width;
                int sizeWidth = (int)g.MeasureString(" (999,999,999 bytes, 100.00%)", font).Width;
                t.rect = new Rectangle(x, y, Math.Max(typeNameWidth, sizeWidth)+dotSize*2, font.Height*2);
                if (maxWidth < t.rect.Width)
                {
                    maxWidth = t.rect.Width;
                }

                y = t.rect.Bottom + typeLegendSpacing;
            }
            int height = y + bottomMargin;
            typeLegendPanel.Height = height;

            int width = leftMargin + maxWidth + rightMargin;
            typeLegendPanel.Width = width;

            x = leftMargin;
            y = topMargin;

            Brush blackBrush = Brushes.Black;

            int dotOffset = (font.Height - dotSize)/2;
            foreach (TypeDesc t in sortedTypeTable)
            {
                g.FillRectangle(t.selected ? blackBrush : t.brush, t.rect.Left, t.rect.Top + dotOffset, dotSize, dotSize);
                g.DrawString(t.typeName, font, blackBrush, t.rect.Left + dotSize*2, t.rect.Top);
                string s = string.Format(" ({0:n0} bytes, {1:f2}%)", t.totalSize, (double)t.totalSize/totalSize*100.0);
                g.DrawString(s, font, blackBrush, t.rect.Left + dotSize*2, t.rect.Top + font.Height);
                y = t.rect.Bottom + typeLegendSpacing;
            }
        }

        private void typeLegendPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            initialized = false;

            if (liveObjectTable == null)
            {
                return;
            }

            Init(e.Graphics);

            DrawTypeLegend(e.Graphics);

            initialized = true;
        }

        private void CheckedChanged(object sender, System.EventArgs e)
        {
            graphPanel.Invalidate();
        }

        private void typeLegendPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!initialized)
            {
                return;
            }

            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
            {
                Debug.Assert(bucketTable != null, "bucketTable != null");
                for (int i = 0; i < bucketTable.Length; i++)
                {
                    if (bucketTable[i].selected)
                    {
                        graphPanel.Invalidate();
                        typeLegendPanel.Invalidate();
                        bucketTable[i].selected = false;
                    }
                }
                if (sortedTypeTable != null)
                {
                    foreach (TypeDesc t in sortedTypeTable)
                    {
                        if (t.rect.Contains(e.X, e.Y) != t.selected)
                        {
                            t.selected = !t.selected;
                            graphPanel.Invalidate();
                            typeLegendPanel.Invalidate();
                        }
                    }
                }
            }
            else if ((e.Button & MouseButtons.Right) != MouseButtons.None)
            {
                var p = new Point(e.X, e.Y);
                contextMenu.Show(typeLegendPanel, p);
            }
        }

        private void graphPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!initialized || verticalScale == 0)
            {
                return;
            }

            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
            {
                if (sortedTypeTable != null)
                {
                    foreach (TypeDesc t in sortedTypeTable)
                    {
                        t.selected = false;
                    }
                }

                int x = leftMargin;
                Debug.Assert(bucketTable != null, "bucketTable != null");
                for (int i = 0; i < bucketTable.Length; i++)
                {
                    bucketTable[i].selected = false;
                    int y = graphPanel.Height - bottomMargin;
                    foreach (TypeDesc t in bucketTable[i].typeDescToSizeCount.Keys)
                    {
                        SizeCount sizeCount = bucketTable[i].typeDescToSizeCount[t];
                        ulong size = sizeCount.size;
                        int height = (int)(size / verticalScale);

                        y -= height;

                        var r = new Rectangle(x, y, bucketWidth, height);
                        if (r.Contains(e.X, e.Y))
                        {
                            t.selected = true;
                            bucketTable[i].selected = true;
                        }
                    }

                    x += bucketWidth + gap;
                }       
                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }
            else if ((e.Button & MouseButtons.Right) != MouseButtons.None)
            {
                var p = new Point(e.X, e.Y);
                contextMenu.Show(graphPanel, p);
            }
        }

        private void graphPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!initialized || verticalScale == 0)
            {
                return;
            }

            if (Form.ActiveForm == this)
            {
                int x = leftMargin;
                Debug.Assert(bucketTable != null, "bucketTable != null");
                foreach (Bucket b in bucketTable)
                {
                    int y = graphPanel.Height - bottomMargin;
                    foreach (KeyValuePair<TypeDesc, SizeCount> d in b.typeDescToSizeCount)
                    {
                        TypeDesc t = d.Key;
                        SizeCount sizeCount = d.Value;
                        ulong size = sizeCount.size;
                        int height = (int)(size / verticalScale);

                        y -= height;

                        var bucketRect = new Rectangle(x, y, bucketWidth, height);
                        if (bucketRect.Contains(e.X, e.Y))
                        {
                            string caption = string.Format("{0} {1} ({2:f2}%) - {3:n0} instances, {4} average size", t.typeName, FormatSize(size), 100.0*size/totalSize, sizeCount.count, FormatSize((uint)(sizeCount.size/sizeCount.count)));
                            toolTip.Active = true;
                            toolTip.SetToolTip(graphPanel, caption);
                            return;
                        }
                    }
                    x += bucketWidth + gap;
                }
            }
            toolTip.Active = false;
            toolTip.SetToolTip(graphPanel, "");
        }

        private readonly bool autoUpdate;

        private void versionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            ReadLogResult readLogResult = MainForm.instance.lastLogResult;

            if (autoUpdate && readLogResult != null && readLogResult.liveObjectTable != liveObjectTable)
            {
                liveObjectTable = readLogResult.liveObjectTable;
                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }       
        }

        private string FormatAge(double age, [CanBeNull] string ageComment)
        {
            if (ageComment == null)
            {
                return FormatTime(age);
            }
            else
            {
                return string.Format("{0} ({1} sec)", ageComment, FormatTime(age));
            }
        }

        private void exportMenuItem_Click(object sender, System.EventArgs e)
        {
            exportSaveFileDialog.FileName = "HistogramByAge.csv";
            exportSaveFileDialog.Filter = "Comma separated files | *.csv";
            if (exportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var w = new StreamWriter(exportSaveFileDialog.FileName);

                TypeDesc selectedType = FindSelectedType();

                string title = "Histogram by Age";
                if (selectedType != null)
                {
                    title += " of " + selectedType.typeName + " objects";
                }

                w.WriteLine(title);
                w.WriteLine();

                w.WriteLine("{0},{1},{2},{3},{4}", "Min Age", "Max Age", "# Instances", "Total Size", "Type");

                bool noBucketSelected = true;
                Debug.Assert(bucketTable != null, "bucketTable != null");
                foreach (Bucket b in bucketTable)
                {
                    if (b.selected)
                    {
                        noBucketSelected = false;
                        break;
                    }
                }

                foreach (Bucket b in bucketTable)
                {
                    if (noBucketSelected || b.selected)
                    {
                        foreach (KeyValuePair<TypeDesc, SizeCount> d in b.typeDescToSizeCount)
                        {
                            TypeDesc t = d.Key;
                            SizeCount sizeCount = d.Value;

                            if (selectedType == null || t == selectedType)
                            {
                                w.WriteLine("{0},{1},{2},{3},{4}", FormatAge(b.minAge, b.minComment), FormatAge(b.maxAge, b.maxComment), sizeCount.count, sizeCount.size, t.typeName);
                            }
                        }
                    }
                }

                w.Close();
            }
        }

        private void showWhoAllocatedMenuItem_Click(object sender, System.EventArgs e)
        {
            TypeDesc selectedType = FindSelectedType();
            double minAge = 0;
            double maxAge = double.PositiveInfinity;
            Debug.Assert(bucketTable != null, "bucketTable != null");
            foreach (Bucket b in bucketTable)
            {
                if (b.selected)
                {
                    minAge = b.minAge;
                    maxAge = b.maxAge;
                }
            }
            string title = "Allocation Graph for objects";
            if (selectedType != null)
            {
                title = string.Format("Allocation Graph for {0} objects", selectedType.typeName);
            }

            if (minAge > 0.0)
            {
                title += string.Format(" of age between {0} and {1} seconds", FormatTime(minAge), FormatTime(maxAge));
            }

            Debug.Assert(liveObjectTable != null, "liveObjectTable != null");
            var selectedHistogram = new Histogram(liveObjectTable.readNewLog);
            LiveObjectTable.LiveObject o;
            double nowTime = liveObjectTable.readNewLog.TickIndexToTime(liveObjectTable.lastTickIndex);
            Debug.Assert(typeIndexToTypeDesc != null, "typeIndexToTypeDesc != null");
            for (liveObjectTable.GetNextObject(0, ulong.MaxValue, out o); o.id < ulong.MaxValue; liveObjectTable.GetNextObject(o.id + o.size, uint.MaxValue, out o))
            {
                double age = nowTime - liveObjectTable.readNewLog.TickIndexToTime(o.allocTickIndex);
                if (minAge <= age && age < maxAge)
                {
                    var t = (TypeDesc)typeIndexToTypeDesc[o.typeIndex];
                
                    if (selectedType == null || t == selectedType)
                    {
                        selectedHistogram.AddObject(o.typeSizeStacktraceIndex, 1);
                    }
                }
            }

            Graph graph = selectedHistogram.BuildAllocationGraph(new FilterForm());

            var graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }

        private void versionTimer_Tick(object sender, System.EventArgs e)
        {
            if (font != MainForm.instance.font)
            {
                font = MainForm.instance.font;
                graphPanel.Invalidate();
                typeLegendPanel.Invalidate();
            }
        }
    }
}

