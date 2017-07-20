/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  PlacedToolTip
 *
 * Description: A tooltip that can be positioned externally
 */

using System;
using System.IO;
using System.Drawing;
using System.Collections;
using System.Windows.Forms;

namespace CLRProfiler
{
    public class PlacedToolTip : Control
    {
        private Font font;
        private int height;
        private string text;
        private int textX, textY;
        private DateTime created;

        private readonly Pen blackPen;

        private readonly System.Timers.Timer timer;

        public string CurrentlyDisplayed()
        {
            return Visible ? text : "";
        }

        public void Display(Point in_location, Font in_font, int in_height, string in_text)
        {
            font = in_font;
            text = in_text;
            height = in_height;

            Location = in_location;

            Graphics g = CreateGraphics();
            SizeF size = g.MeasureString(text, font);
            textX = textY = (int)((height - size.Height) / 2) - 1;
            g.Dispose();

            SuspendLayout();
            Width = (int)(0.5 + size.Width + height - size.Height);
            Height = height;
            ResumeLayout();

            Invalidate();
            Hide();

            created = DateTime.Now;
            timer.Start();
        }

        public PlacedToolTip() : base()
        {
            BackColor = Color.FromArgb(255, 255, 225);
            Hide();

            timer = new System.Timers.Timer(200);
            timer.Elapsed += new System.Timers.ElapsedEventHandler(timerEventHandler);

            blackPen = new Pen(Color.Black);
        }

        protected void timerEventHandler(object s, System.Timers.ElapsedEventArgs e)
        {
            TimeSpan sinceCreated = e.SignalTime.Subtract(created);
            if(!timer.Enabled || sinceCreated.Milliseconds < 500)
            {
                return;
            }

            timer.Stop();
            Point pt = PointToClient(Cursor.Position);
            if(new Rectangle(0, 0, Width, Height).Contains(pt))
            {
                BringToFront();
                Show();
            }
        }

        override protected void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            var size = new Rectangle(new Point(0, 0), new Size(Size.Width - 1, Size.Height - 1));
            g.DrawRectangle(blackPen, size);
            g.DrawString(text, font, Brushes.Black, new Point(textX, textY));
        }

        override protected void OnMouseLeave(EventArgs e)
        {
            Hide();
        }

        override protected void OnMouseDown(MouseEventArgs e)
        {
            Hide();
        }
    }
}
