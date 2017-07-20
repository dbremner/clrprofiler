using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal sealed class ResizeBarCapture : Control
    {
        private int track;
        [NotNull] private readonly List<Column> columns;

        internal ResizeBarCapture([NotNull] [ItemNotNull] List<Column> in_columns) : base()
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
                var left = columns[track];

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