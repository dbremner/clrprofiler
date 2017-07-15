using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace CLRProfiler
{
    internal class ResizeBarCapture : Control
    {
        int track;
        ArrayList columns;

        internal ResizeBarCapture(ArrayList in_columns) : base()
        {
            track = -1;
            columns = in_columns;
        }

        internal void SetCapture(int in_track)
        {
            track = in_track;
            this.Capture = true;
        }

        override protected void OnMouseMove(MouseEventArgs e)
        {
            if(track != -1)
            {
                Column left = (Column)columns[track];

                int ll = left.Left;
                if(e.X < ll + 15)
                {
                    return;
                }
                left.Width = e.X - left.Left;

                ((TreeListView)Parent).RepaintTreeView();
            }
        }

        override protected void OnMouseUp(MouseEventArgs e)
        {
            track = -1;
            this.Capture = false;
        }
    }
}