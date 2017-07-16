using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CLRProfiler
{
    public struct PackageInfo
    {
        public string installedLocation;
        public string architecture;
        public string fullName;
        public string name;
        public string publisher;
        public string version;
        public string tempDir;
        public string acSid;
        public List<AppInfo> appInfoList; 
    }
}