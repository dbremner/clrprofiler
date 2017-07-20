using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace CLRProfiler
{
    internal sealed class Column : Button
    {
        private bool pressed;
        [NotNull] private readonly List<Column> columnsRef;
        [NotNull] private readonly ResizeBarCapture resizeBar;

        [NotNull]
        internal ColumnInformation ColumnInformation { get; }

        internal Column([NotNull] ColumnInformation in_ci, [NotNull] ResizeBarCapture in_resizeBar, [NotNull] List<Column> in_columnsRef) : base()
        {
            pressed = false;

            ColumnInformation = in_ci;
            resizeBar = in_resizeBar;
            columnsRef = in_columnsRef;

            Text = ColumnInformation.Text;

            SetStyle(ControlStyles.Selectable, false);
        }

        override protected void OnMouseMove(MouseEventArgs e)
        {
            Cursor oldCursor = Cursor, toSet = Cursors.Arrow;
            if(pressed)
            {
                base.OnMouseMove(e);
            }
            else
            {
                int index = columnsRef.IndexOf(this);
                int left = 0;
                int right = this.Width;
                if(e.X - left < 5 && index != 0 || right - e.X < 5)
                {
                    toSet = Cursors.VSplit;
                }
            }

            if(oldCursor != toSet)
            {
                Cursor = toSet;
            }
        }

        override protected void OnMouseDown(MouseEventArgs e)
        {
            if(e.Button == MouseButtons.Left)
            {
                int index = columnsRef.IndexOf(this);
                int left = 0;
                int right = this.Width;
                if(e.X - left < 5 && index != 0)
                {
                    resizeBar.SetCapture(index - 1);
                    return;
                }
                else if(right - e.X < 5)
                {
                    resizeBar.SetCapture(index);
                    return;
                }
            }

            pressed = true;
            base.OnMouseDown(e);
        }

        override protected void OnMouseUp(MouseEventArgs e)
        {
            pressed = false;
            base.OnMouseUp(e);
        }
    }
}