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
using System.IO;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for ViewByAddressForm.
    /// </summary>
    public sealed partial class ViewByAddressForm : System.Windows.Forms.Form
    {
        private System.Timers.Timer versionTimer;

        private readonly bool autoUpdate;
        private readonly string baseTitle;

        private Font font;

        internal ViewByAddressForm()
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
            baseTitle = "View Objects by Address";

            ReadLogResult logResult = MainForm.instance.lastLogResult;
            if (logResult != null)
            {
                liveObjectTable = logResult.liveObjectTable;
                typeName = liveObjectTable.readNewLog.typeName;
            }

            font = MainForm.instance.font;
        }

        internal ViewByAddressForm(LiveObjectTable liveObjectTable, string title) : this()
        {
            this.liveObjectTable = liveObjectTable;
            typeName = liveObjectTable.readNewLog.typeName;
            autoUpdate = false;
            baseTitle = title;
            Text = title;
        }

        private readonly Brush blackBrush = new SolidBrush(Color.Black);

        private const int generations = 4;

        private sealed class AddressRange
        {
            internal readonly ulong loAddr;
            internal ulong hiAddr;
            internal readonly AddressRange next;
            internal readonly int index;
            internal readonly ulong[] genLoAddr;
            internal readonly ulong[] genHiAddr;

            internal AddressRange(ulong loAddr, ulong hiAddr, AddressRange next, int index)
            {
                this.loAddr = loAddr;
                this.hiAddr = hiAddr;
                this.next = next;
                this.index = index;
                this.genLoAddr = new ulong[generations];
                this.genHiAddr = new ulong[generations];
                for (int i = 0; i < generations; i++)
                {
                    this.genLoAddr[i] = ulong.MaxValue;
                    this.genHiAddr[i] = ulong.MinValue;
                }
            }
        }

        private const int allowableGap = 1024*1024-1;

        private AddressRange rangeList;
        private int rangeCount = 0;

        private TypeDesc[] typeIndexToTypeDesc;
        private string[] typeName;

        private ArrayList sortedTypeTable;

        private void BuildAddressRangesTypeTable()
        {
            rangeList = null;
            rangeCount = 0;

            if (typeIndexToTypeDesc == null || typeIndexToTypeDesc.Length < typeName.Length)
            {
                typeIndexToTypeDesc = new TypeDesc[typeName.Length];
            }
            else
            {
                foreach (TypeDesc t in typeIndexToTypeDesc)
                {
                    if (t != null)
                    {
                        t.totalSize = 0;
                        t.selectedSize = 0;
                    }
                }
            }

            ulong totalAllocated = 0;
            ulong totalSelected = 0;
            LiveObjectTable.LiveObject o;
            bool prevLOH = false;
            for (liveObjectTable.GetNextObject(0, ulong.MaxValue, out o); o.id < ulong.MaxValue; liveObjectTable.GetNextObject(o.id + o.size, ulong.MaxValue, out o))
            {
                int gen = liveObjectTable.GenerationOfObject(ref o);
                if (gen > generations-1)
                {
                    gen = generations-1;
                }

                bool thisLOH = gen == generations-1;

                // Check whether we can fit this object into the last range we created.
                // We assume the liveObjectList is sorted, so the object can only attach
                // at the end of this range.
                if (rangeList != null && o.id - rangeList.hiAddr <= allowableGap && prevLOH == thisLOH)
                {
                    Debug.Assert(o.id >= rangeList.hiAddr);
                    rangeList.hiAddr = o.id + o.size;
                }
                else
                {
                    rangeList = new AddressRange(o.id, o.id + o.size, rangeList, rangeCount++);
                }
                prevLOH = thisLOH;

                TypeDesc t = typeIndexToTypeDesc[o.typeIndex];
                if (t == null)
                {
                    t = new TypeDesc(o.typeIndex, typeName[o.typeIndex]);
                    typeIndexToTypeDesc[o.typeIndex] = t;
                }
                t.totalSize += o.size;
                totalAllocated += o.size;
                if (InsideSelection(o.id) != 0)
                {
                    t.selectedSize += o.size;
                    totalSelected += o.size;
                }
                
                if (rangeList.genLoAddr[gen] > o.id)
                {
                    rangeList.genLoAddr[gen] = o.id;
                }

                if (rangeList.genHiAddr[gen] < o.id + o.size)
                {
                    rangeList.genHiAddr[gen] = o.id + o.size;
                }
            }

            sortedTypeTable = new ArrayList();
            foreach (TypeDesc t in typeIndexToTypeDesc)
            {
                if (t != null)
                {
                    sortedTypeTable.Add(t);
                }
            }

            sortedTypeTable.Sort();

            foreach (TypeDesc t in sortedTypeTable)
            {
                t.percentage = 0.0;
                if (totalAllocated > 0)
                {
                    t.percentage = 100.0*t.totalSize/totalAllocated;
                }

                t.selectedPercentage = 0.0;
                if (totalSelected > 0)
                {
                    t.selectedPercentage = 100*t.selectedSize/totalSelected;
                }
            }
        }

        private const int typeLegendLeftMargin = 20;
        private int leftMargin = 70;
        private const int bottomMargin = 40;
        private const int clickMargin = 20;
        private const int topMargin = 30;
        private const int rightMargin = 30;
        private const int minHeight = 400;
        private int gap = 50;
        private int dotSize = 8;
        private const int typeLegendSpacing = 3;

        private int LeftMargin(Graphics g)
        {
            return 20 + (int)g.MeasureString("00.0123.4567", font).Width;
        }

        private int Scale(GroupBox groupBox, int pixelsAvailable, int rangeNeeded, bool firstTime)
        {
            if (!firstTime)
            {
                foreach (RadioButton rb in groupBox.Controls)
                {
                    if (rb.Checked)
                    {
                        return Int32.Parse(rb.Text);
                    }
                }
            }
            // No radio button was checked - let's come up with a suitable default
            RadioButton maxLowScaleRB = null;
            int maxLowRange = 0;
            RadioButton minHighScaleRB = null;
            int minHighRange = Int32.MaxValue;
            foreach (RadioButton rb in groupBox.Controls)
            {
                int range = pixelsAvailable*Int32.Parse(rb.Text);
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
                return Int32.Parse(minHighScaleRB.Text);
            }
            else
            {
                Debug.Assert(maxLowScaleRB != null);
                maxLowScaleRB.Checked = true;
                return Int32.Parse(maxLowScaleRB.Text);
            }
        }

        private uint BytesPerPixel(int availablePixels, int neededRange, bool firstTime)
        {
            return (uint)Scale(bytesPerPixelgroupBox, availablePixels, neededRange, firstTime);
        }

        private int HeapWidth(int rangeCount)
        {
            foreach (RadioButton rb in heapWidthGroupBox.Controls)
            {
                if (rb.Checked)
                {
                    return Int32.Parse(rb.Text);
                }
            }
            foreach (RadioButton rb in heapWidthGroupBox.Controls)
            {
                int width = Int32.Parse(rb.Text)*rangeCount;
                if (200 <= width && width < 400)
                {
                    rb.Checked = true;
                    return Int32.Parse(rb.Text);
                }
            }
            onetwoeightRadioButton.Checked = true;
            return 128;
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

        private void ColorTypes()
        {
            bool anyTypesUncolored = false;
            foreach (TypeDesc t in sortedTypeTable)
            {
                anyTypesUncolored |= t.brushes == null;
            }

            if (!anyTypesUncolored)
            {
                return;
            }

            int count = 0;
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

                t.colors = new Color[2];
                t.colors[0] = colors[count];
                t.colors[1] = MixColor(colors[count], Color.Black);
                t.brushes = new Brush[2];
                t.pens = new Pen[2];
                for (int i = 0; i < 2; i++)
                {
                    t.brushes[i] = new SolidBrush(t.colors[i]);
                    t.pens[i] = new Pen(t.colors[i]);
                }
                count++;
            }
        }

        private AddressRange AddressRangeOf(ulong addr)
        {
            AddressRange r;
            for (r = rangeList; r != null; r = r.next)
            {
                if (r.loAddr <= addr && addr <= r.hiAddr)
                {
                    break;
                }
            }
            return r;
        }

        private AddressRange AddressRangeOfObject(ref LiveObjectTable.LiveObject o)
        {
            AddressRange r = AddressRangeOf(o.id);
            if (r != null)
            {
                return r;
            }

            Debug.Assert(false);
            // ReSharper disable once HeuristicUnreachableCode
            rangeList = new AddressRange(o.id, o.id + o.size, rangeList, rangeCount++);

            return rangeList;
        }

        private AddressRange AddressRangeOfObject(ref LiveObjectTable.LiveObject o, AddressRange hint)
        {
            if (hint != null && hint.loAddr <= o.id && o.id < hint.hiAddr)
            {
                return hint;
            }

            return AddressRangeOfObject(ref o);
        }

        private void DrawHorizontalLine(Graphics g, Pen pen, int x1, int x2, int y)
        {
            RectangleF clipRect = g.VisibleClipBounds;
            if (clipRect.Top <= y && y <= clipRect.Bottom)
            {
                IntersectIntervals((int)clipRect.Left, (int)clipRect.Right, ref x1, ref x2);
                if (x1 < x2)
                {
                    g.DrawLine(pen, x1, y, x2, y);
                }
            }
        }

        private void FillSpace(Graphics g, AddressRange r, Pen pen, ulong start, ulong end)
        {
            // and a relative address of the object in this range
            ulong relativeStartAddr = start - r.loAddr;
            ulong relativeEndAddr = end - r.loAddr;

            // divide the relative address by bytesPerPixel to get a pixelAddress
            int pixelStartAddr = (int)(relativeStartAddr / bytesPerPixel);
            int pixelEndAddr = (int)(relativeEndAddr / bytesPerPixel);

            // pixelAddress / heapWidth gives y more or less
            int yAddr = pixelStartAddr / heapWidth;

            // figure out base x for this range
            int baseX = leftMargin + r.index*(heapWidth + gap);

            // figure out how many pixels to draw - there's going to be some rounding error
            int pixelsRemaining = (pixelEndAddr - pixelStartAddr);

            // pixelAddress % heapWidth + baseX gives x
            int xAddr = pixelStartAddr % heapWidth;

            // now draw a line o.size / bytesPerPixel in length at x, y
            // taking care to handle wraparound.
            int x = baseX + xAddr;
            int y = graphPanel.Size.Height - bottomMargin - yAddr;

            while (pixelsRemaining > 0)
            {
                if (xAddr + pixelsRemaining > heapWidth)
                {
                    DrawHorizontalLine(g, pen, x, baseX + heapWidth, y);
                    pixelsRemaining -= heapWidth - xAddr;
                    x = baseX;
                    xAddr = 0;
                    y--;
                }
                else
                {
                    DrawHorizontalLine(g, pen, x, x + pixelsRemaining, y);
                    break;
                }
            }
        }

        private ulong VisibleYToAddress(AddressRange r, int y)
        {
            int relativeY = graphPanel.Size.Height - bottomMargin - y + graphPanel.Top;
            if (relativeY < 0)
            {
                relativeY = 0;
            }

            return r.loAddr + (ulong)relativeY * (ulong)(heapWidth * bytesPerPixel);
        }

        private int InsideSelection(ulong addr)
        {
            if (selectedLowAddr <= addr && addr < selectedHighAddr)
            {
                return 1;
            }
            else
            {
                return 0;
            }
        }

        private const int align = 4;

        private void IntersectIntervals(int aLow, int aHigh, ref int bLow, ref int bHigh)
        {
            bLow = Math.Max(aLow, bLow);
            bHigh = Math.Min(aHigh, bHigh);
        }

        private void IntersectIntervals(ulong aLow, ulong aHigh, ref ulong bLow, ref ulong bHigh)
        {
            bLow = Math.Max(aLow, bLow);
            bHigh = Math.Min(aHigh, bHigh);
        }

        private void DrawLiveObjects(Graphics g, TypeDesc selectedType, ulong lowAddr, ulong highAddr)
        {
            var freePen = new Pen(Color.White);

            RectangleF clipRect = g.VisibleClipBounds;
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                // figure out base x for this range
                int x1 = leftMargin + r.index*(heapWidth + gap);
                int x2 = x1 + heapWidth;
                if (x2 < clipRect.Left || clipRect.Right < x1)
                {
                    continue;
                }

                ulong visibleStartAddr = VisibleYToAddress(r, outerGraphPanel.Size.Height);
                ulong visibleEndAddr = VisibleYToAddress(r, 0);
                if (clipRect.Height < outerGraphPanel.Size.Height)
                {
                    visibleStartAddr = VisibleYToAddress(r, (int)clipRect.Bottom + graphPanel.Top + 1);
                    visibleEndAddr = VisibleYToAddress(r, (int)clipRect.Top + graphPanel.Top - 1);
                }

                IntersectIntervals(lowAddr, highAddr, ref visibleStartAddr, ref visibleEndAddr);
                IntersectIntervals(r.loAddr, r.hiAddr, ref visibleStartAddr, ref visibleEndAddr);

                LiveObjectTable.LiveObject o;
                ulong addr = liveObjectTable.FindObjectBackward(visibleStartAddr);
                liveObjectTable.GetNextObject(addr, visibleStartAddr, out o);
                if (o.id + o.size + align - 1 < visibleStartAddr)
                {
                    addr = visibleStartAddr;
                }

                visibleEndAddr += align - 1;
                for (liveObjectTable.GetNextObject(addr, visibleEndAddr, out o); o.id < visibleEndAddr; liveObjectTable.GetNextObject(addr, visibleEndAddr, out o))
                {
                    // fill any space between this object and the end of the previous one
                    // (or the start of the range, as the case may be) in white
                    if (addr + align - 1 < o.id)
                    {
                        FillSpace(g, r, freePen, addr, o.id);
                    }

                    LiveObjectTable.LiveObject oo;
                    for (addr = o.id + o.size; addr < visibleEndAddr; )
                    {
                        // extend this range if oo is adjacent, of the same type, and still in the range
                        liveObjectTable.GetNextObject(addr, visibleEndAddr, out oo);
                        if (oo.id < visibleEndAddr && addr + align - 1 >= oo.id && oo.typeIndex == o.typeIndex)
                        {
                            addr = oo.id + oo.size;
                        }
                        else
                        {
                            break;
                        }
                    }

                    // figure out what type the object is
                    TypeDesc t = typeIndexToTypeDesc[o.typeIndex];

                    // fill the space in the type's color
                    FillSpace(g, r, t.pens[t.selected], o.id, addr);
                }
                if (addr + align - 1 < visibleEndAddr)
                {
                    FillSpace(g, r, freePen, addr, visibleEndAddr);
                }
            }
        }

        private uint bytesPerPixel;
        private int heapWidth;

        private string FormatAddress(ulong addr)
        {
            if (addr > uint.MaxValue)
            {
                return string.Format("{0:X2}.{1:X4}.{2:X4}", addr >> 32, (addr >> 16) & 0xffff, addr & 0xffff);
            }
            else
            {
                return string.Format("{0:X4}.{1:X4}", (addr >> 16) & 0xffff, addr & 0xffff);
            }
        }


        private void DrawHeapAddress(Graphics g, Brush brush, Pen pen, AddressRange r, ulong addr)
        {
            int baseX = leftMargin + r.index*(heapWidth + gap);

            int baseY = graphPanel.Size.Height - bottomMargin;

            // and a relative address of the object in this range
            ulong relativeAddr = addr - r.loAddr;

            // divide the relative address by bytesPerPixel and heapWidth
            int yAddr = (int)(relativeAddr / (ulong)(bytesPerPixel*heapWidth));

            string label = FormatAddress(addr);

            int width = (int)g.MeasureString(label, font).Width+2;
            int y = baseY - yAddr;
            g.DrawString(label, font, brush, baseX - width, y - font.Height/2);
            g.DrawLine(pen, baseX-3, y, baseX-2, y);
        }

        private void DrawHeapLegend(Graphics g)
        {
            Brush brush = Brushes.Black;
            var pen = Pens.Black;

            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                const int pixelPitch = 64;
    
                ulong addrPitch = (uint)(pixelPitch*bytesPerPixel*heapWidth);

                DrawHeapAddress(g, brush, pen, r, r.loAddr);

                for (ulong addr = (r.loAddr + addrPitch * 3 / 2) / addrPitch * addrPitch; addr < r.hiAddr; addr += addrPitch)
                {
                    DrawHeapAddress(g, brush, pen, r, addr);
                }
            }
        }

        private void DrawGenerationLimits(Graphics g)
        {
            Brush[] brush = new Brush[generations];
            brush[0] = Brushes.Red;
            brush[1] = Brushes.Green;
            brush[2] = Brushes.Blue;
            brush[3] = Brushes.Magenta;
            Pen[] pen = new Pen[generations];
            for (int gen = 0; gen < generations; gen++)
            {
                pen[gen] = new Pen(brush[gen], 3);
                pen[gen].EndCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
                pen[gen].StartCap = System.Drawing.Drawing2D.LineCap.ArrowAnchor;
            }

            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                int baseX = leftMargin + r.index*(heapWidth + gap) + heapWidth + 4;

                int baseY = graphPanel.Size.Height - bottomMargin;

                for (int gen = 0; gen < generations; gen++)
                {
                    if (r.genLoAddr[gen] < r.genHiAddr[gen])
                    {
                        ulong relativeLoAddr = r.genLoAddr[gen] - r.loAddr;
                        ulong relativeHiAddr = r.genHiAddr[gen] - r.loAddr;
                        int yLoAddr = (int)(relativeLoAddr / (ulong)(bytesPerPixel*heapWidth));
                        int yHiAddr = (int)(relativeHiAddr / (ulong)(bytesPerPixel*heapWidth));

                        int yLo = baseY - yLoAddr;
                        int yHi = baseY - yHiAddr;
                        if (yLo - yHi > font.Height)
                        {
                            g.DrawLine(pen[gen], baseX+gen*3, yLo, baseX+gen*3, yHi);
                            string label;
                            if (gen == 3)
                            {
                                label = "LOH";
                            }
                            else
                            {
                                label = string.Format("gen {0}", gen);
                            }

                            g.DrawString(label, font, brush[gen], baseX+gen*3+3, (yLo + yHi - font.Height)/2);
                        }
                    }
                }
            }
        }

        private void DrawTypeDescription(Graphics g, TypeDesc t)
        {
            int dotOffset = (font.Height - dotSize)/2;
            g.FillRectangle(t.brushes[t.selected], t.rect.Left, t.rect.Top+dotOffset, dotSize, dotSize);
            g.DrawString(t.typeName, font, blackBrush, t.rect.Left + dotSize*2, t.rect.Top);
            string s = string.Format(" ({0:n0} bytes, {1:f2}% - {2:n0} bytes, {3:f2}% selected)",
                                         t.totalSize,  t.percentage,      t.selectedSize, t.selectedPercentage);
            g.DrawString(s, font, blackBrush, t.rect.Left + dotSize*2, t.rect.Top + font.Height);
        }

        private void CalculateSelectedTypes()
        {
            foreach (TypeDesc t in sortedTypeTable)
            {
                t.selectedSize = 0;
            }

            ulong totalSelected = 0;
            LiveObjectTable.LiveObject o;
            for (liveObjectTable.GetNextObject(selectedLowAddr, selectedHighAddr, out o); o.id < selectedHighAddr; liveObjectTable.GetNextObject(o.id + o.size, selectedHighAddr, out o))
            {
                TypeDesc t = typeIndexToTypeDesc[o.typeIndex];
                t.selectedSize += o.size;
                totalSelected += o.size;
            }

            foreach (TypeDesc t in sortedTypeTable)
            {
                if (totalSelected != 0)
                {
                    t.selectedPercentage = 100.0*t.selectedSize/totalSelected;
                }
                else
                {
                    t.selectedPercentage = 0.0;
                }
            }

            sortedTypeTable.Sort();
        }

        private void DrawTypeLegend(Graphics g)
        {
            int x = typeLegendLeftMargin;
            int y = topMargin;

            Brush whiteBrush = new SolidBrush(Color.White);
            int dotOffset = (font.Height - dotSize)/2;
            g.FillRectangle(whiteBrush, x, y+dotOffset, dotSize, dotSize);
            g.DrawString("Free space", font, blackBrush, x + dotSize*2, y);
            y += font.Height + typeLegendSpacing;

            foreach (TypeDesc t in sortedTypeTable)
            {
                t.rect = new Rectangle(x, y, (int)g.MeasureString(t.typeName, font).Width+dotSize*2, font.Height*2);
                DrawTypeDescription(g, t);
                y = t.rect.Bottom + typeLegendSpacing;
            }
        }

        private bool initialized = false;

        private LiveObjectTable liveObjectTable;

        private void graphPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            if (!initialized)
            {
                if (autoUpdate)
                {
                    Debug.Assert(MainForm.instance.lastLogResult != null, "MainForm.instance.lastLogResult != null");
                    liveObjectTable = MainForm.instance.lastLogResult.liveObjectTable;
                }

                typeName = liveObjectTable.readNewLog.typeName;

                leftMargin = LeftMargin(g);

                BuildAddressRangesTypeTable();

                ColorTypes();

                gap = (int)g.MeasureString("  gen 0 0123456789", font).Width;

                heapWidth = HeapWidth(rangeCount);

                ulong maxRangeSize = 0;
                for (AddressRange r = rangeList; r != null; r = r.next)
                {
                    ulong rangeSize = r.hiAddr - r.loAddr;
                    if (maxRangeSize < rangeSize)
                    {
                        maxRangeSize = rangeSize;
                    }
                }

                bytesPerPixel = BytesPerPixel(graphPanel.Height - topMargin - bottomMargin, (int)(maxRangeSize/(uint)heapWidth), bytesPerPixel == 0);
            
                int maxHeight = (int)(maxRangeSize/(uint)(bytesPerPixel*heapWidth));
                int height = topMargin + maxHeight + bottomMargin;
                if (height < minHeight)
                {
                    height = minHeight;
                }

                graphPanel.Height = height;

                int width = leftMargin + rangeCount*(heapWidth + gap) + rightMargin;
                graphPanel.Width = width;

                initialized = true;
            }

            g.SetClip(e.ClipRectangle);

            DrawLiveObjects(g, null, 0, ulong.MaxValue);

            var pen = new Pen(Color.Black);
            DrawSelectionHorizontalLine(g, pen, selectedHighAddr);
            DrawSelectionHorizontalLine(g, pen, selectedLowAddr);
            DrawSelectionVerticalLines(g, pen, selectedLowAddr, selectedHighAddr);

            DrawHeapLegend(g);

            DrawGenerationLimits(g);
        }

        private void GraphPanel_Invalidate()
        {
            graphPanel.Invalidate();
        }

        private void Refresh(object sender, System.EventArgs e)
        {
            var rb = (RadioButton)sender;
            if (rb.Checked)
            {
                GraphPanel_Invalidate();
                initialized = false;
            }
        }

        private ulong selectedStartAddr, selectedEndAddr;
        private ulong selectedLowAddr, selectedHighAddr;

        private ulong PixelCoordinatesToAddress(int x, int y)
        {
            // First, find the correct range (if any).
            // we recognize a zone around the ranges as meaning the extreme in that direction
            for (AddressRange r = rangeList; r != null; r = r.next)
            {
                // figure out the boundaries of this particular range
                int minX = leftMargin + r.index*(heapWidth + gap);
                int maxY = graphPanel.Size.Height - bottomMargin;
                int maxX = minX + heapWidth;
                int minY = maxY - (int)((r.hiAddr - r.loAddr)/(uint)(bytesPerPixel*heapWidth));

                if (minX - clickMargin <= x && x <= maxX + clickMargin &&
                    minY - clickMargin <= y && y <= maxY + clickMargin)
                {
                    // This is the right range - limit the coordinates to the actual extremes.
                    if (x < minX)
                    {
                        x = minX;
                    }
                    else if (x > maxX)
                    {
                        x = maxX;
                    }

                    if (y < minY)
                    {
                        y = minY;
                    }
                    else if (y > maxY)
                    {
                        y = maxY;
                    }

                    // Reduce the coordinates to relative coordinates in the range - flipping y
                    x = x - minX;
                    y = maxY - y;

                    // Now figure out the address
                    ulong addr = r.loAddr + (uint)((y * heapWidth + x) * bytesPerPixel);

                    // Limit it so it's within the range of address in r
                    if (addr < r.loAddr)
                    {
                        addr = r.loAddr;
                    }

                    if (addr > r.hiAddr)
                    {
                        addr = r.hiAddr;
                    }

                    return addr;
                }
            }

            return 0;
        }

        private void DrawLiveObjectInterval(Graphics g, ulong low, ulong high)
        {
            if (low < high)
            {
                DrawLiveObjects(g, null, low, high);
            }
        }

        private void DrawLiveObjectIntervals(ulong aLow, ulong aHigh, ulong bLow, ulong bHigh)
        {
            Graphics g = graphPanel.CreateGraphics();

            // draw the live objects in [aLow, aHigh) and [bLow, bHigh)
            // but excluding the ones in the intersection of both intervals
            // caller guarantees aLow <= bLow && aLow <= aHigh && bLow <= bHigh

            Debug.Assert(aLow <= bLow && aLow <= aHigh && bLow <= bHigh);
            if (aHigh <= bLow)
            {
                // this implies two disjoint intervals
                Debug.Assert(aLow <= aHigh && aHigh <= bLow && bLow <= bHigh);
                DrawLiveObjectInterval(g, aLow, aHigh);
                DrawLiveObjectInterval(g, bLow, bHigh);
            }
            else
            {
                Debug.Assert(aLow <= bLow && bLow <= aHigh && bLow <= bHigh);
                if (bHigh <= aHigh)
                {
                    // this implies b interval is nested within a
                    Debug.Assert(aLow <= bLow && bLow <= bHigh && bHigh <= aHigh);
                    DrawLiveObjectInterval(g, aLow, bLow);
                    DrawLiveObjectInterval(g, bHigh, aHigh);
                }
                else
                {
                    // this implies a starts before b, and ends bfore b
                    Debug.Assert(aLow <= bLow && bLow <= aHigh && aHigh <= bHigh);
                    DrawLiveObjectInterval(g, aLow, bLow);
                    DrawLiveObjectInterval(g, aHigh, bHigh);
                }
            }
        }

        private void EraseSelectionHorizontalLine(Graphics g, ulong addr)
        {
            if (addr != 0)
            {
                var backGroundPen = new Pen(graphPanel.BackColor);
                AddressRange r = AddressRangeOf(addr);
                if (r != null)
                {
                    FillSpace(g, r, backGroundPen, addr, addr + (uint)(heapWidth*bytesPerPixel));
                }

                DrawLiveObjectInterval(g, addr, addr + (uint)(heapWidth*bytesPerPixel));
            }
        }

        private void DrawSelectionHorizontalLine(Graphics g, Pen pen, ulong addr)
        {
            AddressRange r = AddressRangeOf(addr);
            if (r != null)
            {
                FillSpace(g, r, pen, addr, addr + (uint)(heapWidth*bytesPerPixel));
            }
        }

        private void DrawSelectionVerticalLines(Graphics g, Pen pen, ulong lowAddr, ulong highAddr)
        {
            AddressRange r = AddressRangeOf(lowAddr);
            if (r == null)
            {
                return;
            }

            int baseX = leftMargin + r.index*(heapWidth + gap);

            int baseY = graphPanel.Size.Height - bottomMargin;

            // and a relative address of the object in this range
            ulong relativeLowAddr = lowAddr - r.loAddr;
            ulong relativeHighAddr = highAddr - r.loAddr;

            // divide the relative address by bytesPerPixel and heapWidth
            int yLowAddr = (int)(relativeLowAddr / (ulong)(bytesPerPixel*heapWidth));
            int yHighAddr = (int)(relativeHighAddr / (ulong)(bytesPerPixel*heapWidth));

            int lowY = baseY - yLowAddr;
            int highY = baseY - yHighAddr;
            g.DrawLine(pen, baseX-1, lowY-1, baseX-1, highY-1);
            g.DrawLine(pen, baseX+heapWidth+1, lowY, baseX+heapWidth+1, highY);
        }

        private void SetTitle()
        {
            TypeDesc selectedType = FindSelectedType();
            string title = baseTitle;
            if (selectedType != null || selectedLowAddr != 0)
            {
                title += " - selected: " + ComputeObjectsDescription(selectedType, selectedLowAddr, selectedHighAddr);
            }

            Text = title;
        }

        private void SetSelection()
        {
            ulong oldLowAddr = selectedLowAddr;
            ulong oldHighAddr = selectedHighAddr;
            if (selectedStartAddr < selectedEndAddr)
            {
                selectedLowAddr = selectedStartAddr;
                selectedHighAddr = selectedEndAddr;
            }
            else
            {
                selectedLowAddr = selectedEndAddr;
                selectedHighAddr = selectedStartAddr;
            }
            if (oldLowAddr != selectedLowAddr || oldHighAddr != selectedHighAddr)
            {
                // we need to paint what's different, i.e. whatever is in
                // the old selection but not the new, and vice versa.
/*
                if (oldLowAddr < selectedLowAddr)
                    DrawLiveObjectIntervals(oldLowAddr, oldHighAddr, selectedLowAddr, selectedHighAddr);
                else
                    DrawLiveObjectIntervals(selectedLowAddr, selectedHighAddr, oldLowAddr, oldHighAddr);
*/
                Graphics g = graphPanel.CreateGraphics();
                var blackPen = new Pen(Color.Black);
                var backGroundPen = new Pen(graphPanel.BackColor);
                if (oldLowAddr != selectedLowAddr)
                {
                    EraseSelectionHorizontalLine(g, oldLowAddr);
                }

                if (oldHighAddr != selectedHighAddr)
                {
                    EraseSelectionHorizontalLine(g, oldHighAddr);
                }

                DrawSelectionHorizontalLine(g, blackPen, selectedHighAddr);
                DrawSelectionHorizontalLine(g, blackPen, selectedLowAddr);
                if (oldLowAddr == selectedLowAddr)
                {
                    if (oldHighAddr < selectedHighAddr)
                    {
                        DrawSelectionVerticalLines(g, blackPen, oldHighAddr, selectedHighAddr);
                    }
                    else
                    {
                        DrawSelectionVerticalLines(g, backGroundPen, selectedHighAddr, oldHighAddr);
                    }
                }
                else if (oldHighAddr == selectedHighAddr)
                {
                    if (oldLowAddr < selectedLowAddr)
                    {
                        DrawSelectionVerticalLines(g, backGroundPen, oldLowAddr, selectedLowAddr);
                    }
                    else
                    {
                        DrawSelectionVerticalLines(g, blackPen, selectedLowAddr, oldLowAddr);
                    }
                }
                else
                {
                    DrawSelectionVerticalLines(g, backGroundPen, oldLowAddr, oldHighAddr);
                    DrawSelectionVerticalLines(g, blackPen, selectedLowAddr, selectedHighAddr);
                }
                SetTitle();
                CalculateSelectedTypes();
                typeLegendPanel.Invalidate();
            }
        }

        private void graphPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
            {
                if ((Control.ModifierKeys & Keys.Shift) != 0)
                {
                    selectedEndAddr = PixelCoordinatesToAddress(e.X, e.Y);
                    SetSelection();
                }
                else
                {
                    selectedStartAddr = PixelCoordinatesToAddress(e.X, e.Y);
                    if (selectedEndAddr != 0)
                    {
                        selectedEndAddr = selectedStartAddr;
                        SetSelection();
                    }
                }
            }
            else if ((e.Button & MouseButtons.Right) != MouseButtons.None)
            {
                var p = new Point(e.X, e.Y);
                contextMenu.Show(graphPanel, p);
            }
        }

        private bool FindObject(ulong addr, out LiveObjectTable.LiveObject o)
        {
            ulong id = liveObjectTable.FindObjectBackward(addr);
            liveObjectTable.GetNextObject(id, addr + 4, out o);
            return o.id <= addr && addr < o.id + o.size;
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

        private void graphPanel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (!initialized)
            {
                return;
            }

            if (e.Button == MouseButtons.None)
            {
                LiveObjectTable.LiveObject o;
                if (Form.ActiveForm == this && FindObject(PixelCoordinatesToAddress(e.X, e.Y), out o))
                {
                    ReadNewLog log = liveObjectTable.readNewLog;
                    double age = (log.TickIndexToTime(liveObjectTable.lastTickIndex) - log.TickIndexToTime(o.allocTickIndex));
                    string caption = string.Format("{0} at {1} using {2} - {3:f3} secs old", typeIndexToTypeDesc[o.typeIndex].typeName, FormatAddress(o.id), FormatSize(o.size), age);
                    toolTip.Active = true;
                    toolTip.SetToolTip(graphPanel, caption);
                }
                else
                {
                    toolTip.Active = false;
                    toolTip.SetToolTip(graphPanel, "");
                }
                return;
            }

            toolTip.Active = false;
            toolTip.SetToolTip(graphPanel, "");

            // So the mouse is moving with the left button down
            // Presumably the user wants to make a selection
            if (selectedStartAddr != 0)
            {
                ulong newEndAddr = PixelCoordinatesToAddress(e.X, e.Y);
                if (newEndAddr != selectedEndAddr)
                {
                    selectedEndAddr = newEndAddr;
                    SetSelection();
                }
            }
        }

        private void graphPanel_MouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if (e.Button == MouseButtons.None)
            {
                if (selectedStartAddr != 0)
                {
                    selectedEndAddr = PixelCoordinatesToAddress(e.X, e.Y);
                }

                SetSelection();
            }
        }

        private void typeLegendPanel_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            if (!initialized)
            {
                BuildAddressRangesTypeTable();
                ColorTypes();
            }

            Graphics g = e.Graphics;

            int maxTypeNameWidth = 0;
            foreach (TypeDesc t in sortedTypeTable)
            {
                int typeWidth = (int)g.MeasureString(t.typeName, font).Width;
                if (maxTypeNameWidth < typeWidth)
                {
                    maxTypeNameWidth = typeWidth;
                }

                typeWidth = (int)g.MeasureString(" (999,999,999 bytes, 99.99% - 999,999,999 bytes, 99.99% selected)", font).Width;
                if (maxTypeNameWidth < typeWidth)
                {
                    maxTypeNameWidth = typeWidth;
                }
            }
            dotSize = (int)g.MeasureString("0", font).Width;

            int width = leftMargin + dotSize + maxTypeNameWidth + rightMargin;

            typeLegendPanel.Width = width;

            int height = topMargin + sortedTypeTable.Count*(font.Height*2 + typeLegendSpacing) + bottomMargin;

            typeLegendPanel.Height = height;

            DrawTypeLegend(g);  
        }

        private void RedrawType(TypeDesc t)
        {
            DrawTypeDescription(typeLegendPanel.CreateGraphics(), t);
            Graphics g = graphPanel.CreateGraphics();
            DrawLiveObjects(g, t, 0, ulong.MaxValue);
            var pen = Pens.Black;
            DrawSelectionHorizontalLine(g, pen, selectedHighAddr);
            DrawSelectionHorizontalLine(g, pen, selectedLowAddr);
        }

        private void typeLegendPanel_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            if ((e.Button & MouseButtons.Left) != MouseButtons.None)
            {
                foreach (TypeDesc t in sortedTypeTable)
                {
                    if (t.rect.Contains(e.X, e.Y) || t.selected != 0)
                    {
                        t.selected = 1 - t.selected;
                        RedrawType(t);
                        SetTitle();
                    }
                }
            }
            else if ((e.Button & MouseButtons.Right) != MouseButtons.None)
            {
                var p = new Point(e.X, e.Y);
                contextMenu.Show(typeLegendPanel, p);
            }
        }

        private void versionTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (font != MainForm.instance.font)
            {
                font = MainForm.instance.font;
                initialized = false;
                GraphPanel_Invalidate();
                typeLegendPanel.Invalidate();
            }

            if (!autoUpdate)
            {
                return;
            }

            ReadLogResult logResult = MainForm.instance.lastLogResult;
            if (logResult != null && logResult.liveObjectTable != liveObjectTable)
            {
                liveObjectTable = logResult.liveObjectTable;
                initialized = false;
                GraphPanel_Invalidate();
                typeLegendPanel.Invalidate();
            }
        }

        private TypeDesc FindSelectedType()
        {
            // Figure out whether there's a selected type
            foreach (TypeDesc t in sortedTypeTable)
            {
                if (t.selected != 0)
                {
                    return t;
                }
            }
            return null;
        }

        private string ComputeObjectsDescription(TypeDesc selectedType, ulong selectedLowAddr, ulong selectedHighAddr)
        {
            string description = "";
            if (selectedType != null)
            {
                description += selectedType.typeName + " ";
            }

            description += "objects ";
            if (selectedLowAddr > 0)
            {
                description += string.Format("between {0} and {1}", FormatAddress(selectedLowAddr), FormatAddress(selectedHighAddr));
            }

            return description;
        }

        private void showAllocatorsMenuItem_Click(object sender, System.EventArgs e)
        {
            TypeDesc selectedType = FindSelectedType();

            // Create a new allocation graph and add all the objects in the selected address range
            // whose type matches the selected type (if any).

            ReadNewLog log = liveObjectTable.readNewLog;
            var histogram = new Histogram(log);
            ulong low = selectedLowAddr;
            ulong high = low == 0 ? ulong.MaxValue : selectedHighAddr;
            LiveObjectTable.LiveObject o;
            for (liveObjectTable.GetNextObject(low, high, out o); o.id < high; liveObjectTable.GetNextObject(o.id + o.size, high, out o))
            {
                if (selectedType == null || selectedType.typeIndex == o.typeIndex)
                {
                    histogram.AddObject(o.typeSizeStacktraceIndex, 1);
                }
            }

            // Build the real graph from the histogram

            Graph graph = histogram.BuildAllocationGraph(new FilterForm());

            // And make another graph form for it - hardest part is to compute an appropriate title...

            string title = "Allocation Graph for live " + ComputeObjectsDescription(selectedType, selectedLowAddr, selectedHighAddr);
            var graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }

        private void showHistogramMenuItem_Click(object sender, System.EventArgs e)
        {
            TypeDesc selectedType = FindSelectedType();

            // Create a new histogram and add all the objects in the selected address range
            // whose type matches the selected type (if any).

            ReadNewLog log = liveObjectTable.readNewLog;
            var histogram = new Histogram(log);
            ulong low = selectedLowAddr;
            ulong high = low == 0 ? ulong.MaxValue : selectedHighAddr;
            LiveObjectTable.LiveObject o;
            for (liveObjectTable.GetNextObject(low, high, out o); o.id < high; liveObjectTable.GetNextObject(o.id + o.size, high, out o))
            {
                if (selectedType == null || selectedType.typeIndex == o.typeIndex)
                {
                    histogram.AddObject(o.typeSizeStacktraceIndex, 1);
                }
            }

            string title = "Histogram by Size for live " + ComputeObjectsDescription(selectedType, selectedLowAddr, selectedHighAddr);
            var histogramViewForm = new HistogramViewForm(histogram, title);
            histogramViewForm.Show();
        }

        private void exportMenuItem_Click(object sender, System.EventArgs e)
        {
            exportSaveFileDialog.FileName = "LiveObjects.csv";
            exportSaveFileDialog.Filter = "Comma separated files | *.csv";
            if (exportSaveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var w = new StreamWriter(exportSaveFileDialog.FileName);

                TypeDesc selectedType = FindSelectedType();

                string title = "List of" + ComputeObjectsDescription(selectedType, selectedLowAddr, selectedHighAddr);

                w.WriteLine(title);
                w.WriteLine();

                w.WriteLine("{0},{1},{2},{3},{4},{5}", "Address", "Size", "Type", "Age", "Allocated by", "Called from");

                ulong low = selectedLowAddr;
                ulong high = low == 0 ? ulong.MaxValue : selectedHighAddr;
                LiveObjectTable.LiveObject o;
                ReadNewLog log = liveObjectTable.readNewLog;
                for (liveObjectTable.GetNextObject(low, high, out o); o.id < high; liveObjectTable.GetNextObject(o.id + o.size, high, out o))
                {
                    if (selectedType != null && selectedType.typeIndex != o.typeIndex)
                    {
                        continue;
                    }

                    double age = (log.TickIndexToTime(liveObjectTable.lastTickIndex) - log.TickIndexToTime(o.allocTickIndex));
                    w.Write("{0},{1},{2},{3:f3}", FormatAddress(o.id), o.size, typeName[o.typeIndex], age);

                    int[] stacktrace = log.stacktraceTable.IndexToStacktrace(o.typeSizeStacktraceIndex);
                    for (int i = 2; i < stacktrace.Length; i++)
                    {
                        w.Write(",{0}", log.funcName[stacktrace[i]]);
                    }
                    w.WriteLine();
                }

                w.Close();
            }
        }
    }
}
