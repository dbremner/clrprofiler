using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace CLRProfiler
{
    internal sealed class ConsoleCtrl
    {
        internal enum ConsoleEvent
        {
            CTRL_C = 0,           // From wincom.h
            CTRL_BREAK = 1,
            CTRL_CLOSE = 2,
            CTRL_LOGOFF = 5,
            CTRL_SHUTDOWN = 6
        }

        internal delegate bool ControlEventHandler(ConsoleEvent consoleEvent);

        internal event ControlEventHandler ControlEvent;

        private readonly ControlEventHandler eventHandler;

        internal ConsoleCtrl()
        {
            // save this to a private var so the GC doesn't collect it...
            eventHandler = new ControlEventHandler(Handler);
            SetConsoleCtrlHandler(eventHandler, true);
        }

        private bool Handler(ConsoleEvent consoleEvent)
        {
            if (ControlEvent != null)
            {
                return ControlEvent(consoleEvent);
            }

            return false;
        }

        [DllImport("kernel32.dll")]
        private static extern bool SetConsoleCtrlHandler(ControlEventHandler e, bool add);
    }
}