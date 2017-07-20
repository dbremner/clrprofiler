using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    internal sealed class WindowsStoreAppProfileeInfo
    {
        public WindowsStoreAppProfileeInfo(string packageFullNameParam, string acSidString)
        {
            packageFullName = packageFullNameParam;
            windowsStoreAppEventPrefix = String.Format("AppContainerNamedObjects\\{0}\\", acSidString);
        }

        public readonly string windowsStoreAppEventPrefix;
        public readonly string packageFullName;
    }
}