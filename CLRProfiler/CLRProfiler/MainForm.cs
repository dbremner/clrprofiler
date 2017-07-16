/* ==++==
 * 
 *   Copyright (c) Microsoft Corporation.  All rights reserved.
 * 
 * ==--==
 *
 * Class:  Form1
 *
 * Description: CLR Profiler interface and logic
 */
using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.IO;
using System.Threading;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Globalization;
using System.Xml;
using JetBrains.Annotations;
using Microsoft.Win32.SafeHandles;

namespace CLRProfiler
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public partial class MainForm : System.Windows.Forms.Form
    {

        internal Font font;
        private Process profiledProcess;
        private string processFileName;
        private string serviceName;
        private string serviceAccountSid;
        private string serviceStartCommand;
        private string serviceStopCommand;
        private string logFileName;
        private long logFileStartOffset;
        private long logFileEndOffset;
        internal ReadNewLog log;
        [CanBeNull] internal ReadLogResult lastLogResult;
        private NamedManualResetEvent loggingActiveEvent;
        private NamedManualResetEvent forceGcEvent;
        private NamedManualResetEvent loggingActiveCompletedEvent;
        private NamedManualResetEvent forceGcCompletedEvent;
        private NamedManualResetEvent callGraphActiveEvent;
        private NamedManualResetEvent callGraphActiveCompletedEvent;
        private NamedManualResetEvent detachEvent;
        private WindowsStoreAppProfileeInfo windowsStoreAppProfileeInfo;
        private string commandLine = "";
        private string workingDirectory = "";
        private string logDirectory;
        private int attachTargetPID;
        private readonly SafeFileHandle handshakingPipeHandle;
        private readonly FileStream handshakingPipe;
        private readonly SafeFileHandle loggingPipeHandle;
        private readonly FileStream loggingPipe;
        internal static MainForm instance;
        private bool saveNever;
        private bool gcOnLogFileComments;

        internal bool noUI = true;
        private readonly string nameToUse;
        private readonly bool profileAllocations;
        private readonly bool profileCalls;
        private readonly bool profilingActive;
        private readonly bool trackCallStacks = true;
        private bool profilerConnected = false;
        private readonly CLRSKU targetCLRVersion = CLRSKU.V4DesktopCLR;
        private string profilingURL;

        private readonly bool help;
        internal string prevlogFileName;
        internal string currlogFileName;
        internal readonly string difflogFileName;
        internal Graph.GraphType graphtype = Graph.GraphType.Invalid;
        internal readonly bool viewdiff = false;
        internal bool exitProgram = false;

        private readonly ReportKind reportKind;
        private readonly string startMarker;
        private readonly string endMarker;
        private readonly string[] timeMarker = new string[0];

        private readonly uint maxWaitingTimeInMiliseconds = 10000;

        internal bool IsProfilingWindowsStoreApp()
        {
            return (windowsStoreAppProfileeInfo != null);
        }


        internal void CommandLineError(string message)
        {
            if (!noUI)
            {
                MessageBox.Show(message, "Report Profiler");
                ShowUsage();
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        internal void CommandLineError(string message, params object[] args)
        {
            CommandLineError(string.Format(message, args));
        }

        internal string FetchArgument(string[] arguments, ref int i)
        {
            i++;
            if (arguments.Length > i)
            {
                return arguments[i];
            }
            else
            {
                CommandLineError("Option {0} expects an argument", arguments[i - 1]);
                return "";
            }
        }

        public MainForm(string[] arguments)
        {
            int i;
            string logFileName = "";
            bool onlyCreateLog = false;
            processFileName = null;
            windowsStoreAppProfileeInfo = null;

            profileAllocations = profileCalls = profilingActive = true;

            //Check ProfilerOBJ.DLL exist first
            if (!File.Exists( getProfilerFullPath() ) )
            {
                ShowErrorMessage("Can not find '" + getProfilerFullPath() + "', please make sure ProfilerOBJ.dll exists at the same directory as CLRProfiler.exe.");
                Environment.ExitCode = 1;
                exitProgram = true;
                return;
            }

            #region arguments
            if (arguments.Length > 0)
            {
                int pid;

                Environment.ExitCode = 0;

                switch (arguments[0])
                {
                    case "-attach":
                    case "/attach":
                        exitProgram = true;
                        if ((pid = getPID(arguments)) == 0)
                        {
                            Environment.ExitCode = 1;
                            return;
                        }

                        for (int index = 2; index + 1 < arguments.Length; index = index+2)
                        { 
                            if (arguments[index] == "-o" || arguments[index] == "/o")
                            {
                                string fullPath = Path.GetFullPath(arguments[index+1]);
                                logDirectory = Path.GetDirectoryName(fullPath);
                                nameToUse = Path.GetFileName(fullPath);
                            }

                            if (arguments[index] == "-t" || arguments[index] == "/t")
                            {
                                maxWaitingTimeInMiliseconds = UInt32.Parse(arguments[index + 1]);
                                if (maxWaitingTimeInMiliseconds < 3000)
                                {
                                    maxWaitingTimeInMiliseconds = 3000;
                                    ShowErrorMessage("CLRProfiler must wait a minimum of 3000 milliseconds before it will time out while attaching to the application.");
                                }
                            }
                        }

                        if(logDirectory == null)
                        {
                            logDirectory = Directory.GetCurrentDirectory();
                        }
                        /* basic initialization stuff */
                        CreatePipe(@"\\.\pipe\OMV_PIPE", false, ref handshakingPipeHandle, ref handshakingPipe);
                        CreatePipe(@"\\.\pipe\OMV_LOGGING_PIPE", false, ref loggingPipeHandle, ref loggingPipe);
                        instance = this;

                        if (attachProfiler(pid, GetLogFullPath(pid)) )
                        {
                            Console.WriteLine("The logging data is saved at " + GetLogFullPath(pid) + ".");
                        }
                        else
                        {
                            Environment.ExitCode = 1;
                        }

                        ClearProfiledProcessInfo();
                        return;

                    case "-detach":
                    case "/detach":
                        exitProgram = true;

                        if ((pid = getPID(arguments)) == 0)
                        {
                            Environment.ExitCode = 1;
                            return;
                        }

                        if (!InitWindowsStoreAppProfileeInfoIfNecessary(pid))
                        {
                            return;
                        }

                        if (!isProfilerLoaded(pid))
                        {
                            Console.WriteLine("PID argument is not valid.");
                            Environment.ExitCode = 1;
                            return;
                        }

                        CreateEvents(pid);

                        SendDetachRequest();

                        Console.WriteLine("Detach is sucessfully requested.");
                        Console.WriteLine("It may take a while for CLRProfiler to be unloaded completely.  ");
                        Console.WriteLine("You may look for profiler detach event from the event log to determine if detach succeeds.");

                        ClearProfiledProcessInfo();
                        return;

                    case "-dumpheap":
                    case "/dumpheap":
                        exitProgram = true;

                        if ((pid = getPID(arguments)) == 0)
                        {
                            Environment.ExitCode = 1;
                            return;
                        }

                        if (!InitWindowsStoreAppProfileeInfoIfNecessary(pid))
                        {
                            return;
                        }

                        if (!isProfilerLoaded(pid))
                        {
                            Console.WriteLine("PID argument is not valid.");
                            Environment.ExitCode = 1;
                            return;
                        }

                        CreateEvents(pid);
                        forceGcCompletedEvent.Wait(1);
                        forceGcCompletedEvent.Reset();
                        forceGcEvent.Set();
                        Console.WriteLine(Path.GetFileName(Application.ExecutablePath) + " is waiting up to 10 minutes for target process to finish dumping the GC heap.");
                        if (forceGcCompletedEvent.Wait(10 * 60 * 1000))
                        {
                            forceGcCompletedEvent.Reset();
                            Console.WriteLine("A heap dump is appended in the log file.");
                        }
                        else
                        {
                            Console.WriteLine("There was no response from the target process.");
                            Environment.ExitCode = 1;
                        }
                        ClearEvents();
                        ClearProfiledProcessInfo();
                        return;
                }
            }
            #endregion arguments
            #region arguments2
            for (i = 0; i < arguments.Length && !onlyCreateLog; i++)
            {
                switch (arguments[i])
                {
                    case "-target":
                    case "/target":
                        string version = FetchArgument(arguments, ref i);

                        if (String.Compare(version, "V4DesktopCLR", true) == 0)
                        {
                            targetCLRVersion = CLRSKU.V4DesktopCLR;
                        }
                        else if (String.Compare(version, "V4CoreCLR", true) == 0)
                        {
                            targetCLRVersion = CLRSKU.V4CoreCLR;
                        }
                        else if (String.Compare(version, "V2DesktopCLR", true) == 0)
                        {
                            targetCLRVersion = CLRSKU.V2DesktopCLR;
                        }
                        else
                        {
                            CommandLineError("No matched target CLR version.");
                            help = true;
                        }
                        break;

                    case "-u":
                    case "/u":
                        if (logFileName == null)
                        {
                            CommandLineError("A log filename needs to be specified with -o beofre -u argument");
                            help = true;
                        }
                        else
                        {
                            profilingURL = FetchArgument(arguments, ref i);
                            if (profilingURL != null)
                            {
                                onlyCreateLog = true;
                            }
                        }
                        break;

                    case "-na":
                    case "/na":
                        profileAllocations = false;
                        break;

                    case "-nc":
                    case "/nc":
                        profileCalls = false;
                        break;

                    case "-np":
                    case "/np":
                        profilingActive = false;
                        break;

                    case "/ns":
                    case "-ns":
                        trackCallStacks = false;
                        break;

                    case "-o":
                    case "/o":
                        logFileName = FetchArgument(arguments, ref i);
                        break;

                    case "-p":
                    case "/p":
                        processFileName = FetchArgument(arguments, ref i);
                        if (processFileName != null)
                        {
                            onlyCreateLog = true;
                        }

                        break;

                    case "-diff":
                    case "/diff":
                        graphtype = Graph.GraphType.AllocationGraph;
                        break;

                    case "-lo":
                    case "/lo":
                        prevlogFileName = FetchArgument(arguments, ref i);
                        break;

                    case "-ln":
                    case "/ln":
                        currlogFileName = FetchArgument(arguments, ref i);
                        break;

                    case "-ld":
                    case "/ld":
                        difflogFileName = FetchArgument(arguments, ref i);
                        break;

                    case "-l":
                    case "/l":
                        logFileName = FetchArgument(arguments, ref i);
                        break;

                    case "-a":
                    case "/a":
                        reportKind = ReportKind.AllocationReport;
                        break;

                    case "-r":
                    case "/r":
                        reportKind = ReportKind.RelocationReport;
                        break;

                    case "-s":
                    case "/s":
                        reportKind = ReportKind.SurvivorReport;
                        break;

                    case "-sd":
                    case "/sd":
                        reportKind = ReportKind.SurvivorDifferenceReport;
                        break;

                    case "-h":
                    case "/h":
                        reportKind = ReportKind.HeapDumpReport;
                        break;

                    case "-lr":
                    case "/lr":
                        reportKind = ReportKind.LeakReport;
                        break;

                    case "-f":
                    case "/f":
                        reportKind = ReportKind.FinalizerReport;
                        break;

                    case "-cf":
                    case "/cf":
                        reportKind = ReportKind.CriticalFinalizerReport;
                        break;

                    case "-c":
                    case "/c":
                        reportKind = ReportKind.CommentReport;
                        break;

                    case "-b":
                    case "/b":
                        startMarker = FetchArgument(arguments, ref i);
                        break;

                    case "-e":
                    case "/e":
                        endMarker = FetchArgument(arguments, ref i);
                        break;

                    case "-t":
                    case "/t":
                        do
                        {
                            string[] newTimeMarker = new String[timeMarker.Length + 1];
                            for (int j = 0; j < timeMarker.Length; j++)
                            {
                                newTimeMarker[j] = timeMarker[j];
                            }

                            newTimeMarker[timeMarker.Length] = FetchArgument(arguments, ref i);
                            timeMarker = newTimeMarker;
                        }
                        while (i + 1 < arguments.Length && arguments[i + 1][0] != '-' && arguments[i + 1][0] != '/');
                        break;

                    case "-w":
                    case "/w":
                        this.noUI = false;
                        break;

                    case "-?":
                    case "/?":
                    case "-help":
                    case "/help":
                        help = true;
                        break;

                    case "-gc":
                    case "/gc":
                        gcOnLogFileComments = true;
                        break;

                    default:
                        if (arguments[i][0] == '-' || arguments[i][0] == '/')
                        {
                            CommandLineError("Unrecognized option {0}", arguments[i]);
                        }
                        else if (File.Exists(arguments[i]))
                        {
                            logFileName = arguments[i];
                        }
                        else
                        {
                            CommandLineError("Log file {0} not found", arguments[i]);
                        }

                        break;
                }
            }
            #endregion arguments2
            #region AllocationGraph
            if (graphtype == Graph.GraphType.AllocationGraph)
            {
                if (File.Exists(prevlogFileName) && File.Exists(currlogFileName))
                {
                    viewdiff = true;
                    return;
                }
                else
                {
                    String s = "";
                    if (!File.Exists(prevlogFileName))
                    {
                        s += String.Format("Previous build log File not exists or command line missing -lo. \n");
                    }
                    if (!File.Exists(currlogFileName))
                    {
                        s += String.Format("New build log file not exists or command line missing -ln \n");
                    }
                    CommandLineError(s);
                }
            }
            #endregion AllocationGraph
            #region NoReport
            else if (reportKind != ReportKind.NoReport)
            {
                if (logFileName == "")
                {
                    CommandLineError("Need -l logFileName for report");
                }
                else if (!File.Exists(logFileName))
                {
                    CommandLineError("Log file {0} not found", logFileName);
                }
                else
                {
                    switch (reportKind)
                    {
                        case ReportKind.AllocationReport:
                            Reports.AllocationReport(logFileName, startMarker, endMarker);
                            break;

                        case ReportKind.RelocationReport:
                            Reports.RelocationReport(logFileName, startMarker, endMarker);
                            break;

                        case ReportKind.SurvivorReport:
                            Reports.SurvivorReport(logFileName, startMarker, endMarker, timeMarker);
                            break;

                        case ReportKind.SurvivorDifferenceReport:
                            Reports.SurvivorDifferenceReport(logFileName, startMarker, endMarker);
                            break;

                        case ReportKind.HeapDumpReport:
                            Reports.HeapDumpReport(logFileName, startMarker, endMarker);
                            break;

                        case ReportKind.LeakReport:
                            Reports.LeakReport(logFileName, startMarker, endMarker);
                            break;

                        case ReportKind.FinalizerReport:
                            Reports.FinalizerReport(false, logFileName, startMarker, endMarker);
                            break;

                        case ReportKind.CriticalFinalizerReport:
                            Reports.FinalizerReport(true, logFileName, startMarker, endMarker);
                            break;

                        case ReportKind.CommentReport:
                            Reports.CommentReport(logFileName);
                            break;
                    }
                }
            }
            #endregion NoReport
            else
            {
                if (onlyCreateLog)
                {
                    /* treat everything after the exe name to profile as arguments */
                    for (; i < arguments.Length; i++)
                    {
                        commandLine += arguments[i] + ' ';
                    }
                }
                /* basic initialization stuff */
                CreatePipe(@"\\.\pipe\OMV_PIPE", false, ref handshakingPipeHandle, ref handshakingPipe);
                CreatePipe(@"\\.\pipe\OMV_LOGGING_PIPE", false, ref loggingPipeHandle, ref loggingPipe);
                instance = this;

                if (!onlyCreateLog)
                {
                    /* standard UI operation */
                    if (help)
                    {
                        ShowUsage();
                        exitProgram = true;
                        return;
                    }
                    noUI = false;
                    //
                    // Required for Windows Form Designer support
                    //
                    InitializeComponent();

                    // Set V4 Desktop CLR to be the default value
                    targetCLRVersioncomboBox.SelectedIndex = 0;

                    font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(204))); ;

                    if (logFileName != "" && File.Exists(logFileName))
                    {
                        LoadLogFile(logFileName);
                    }

                    EnableDisableViewMenuItems();
                }
                else
                {
                    /* command-line only */
                    noUI = true;
                    exitProgram = true;

                    try
                    {
                        string fileName = (logFileName == "" ? Path.ChangeExtension(processFileName, ".log") : logFileName);
                        string fullPath = Path.GetFullPath(fileName);
                        logDirectory = Path.GetDirectoryName(fullPath);
                        nameToUse = Path.GetFileName(fileName);

                        if (profilingURL == null)
                        {
                            startApplicationButton_Click(null, null);
                        }
                        else
                        {
                            startURLButton_Click(null, null);
                        }

                        if (profilerConnected)
                        {
                            Environment.ExitCode = 0;
                        }
                        else
                        {
                            Environment.ExitCode = 1;
                        }

                        while (profiledProcess != null && !ProfiledProcessHasExited())
                        {
                            Thread.Sleep(500);
                        }
                    }
                    catch (Exception e)
                    {
                        throw new Exception(e.Message + '\n' + e.StackTrace);
                        // Console.WriteLine("There was a problem profiling {0}", processFileName);
                    }
                }
            }
        }

        private void ShowUsage()
        {
            graphtype = Graph.GraphType.Invalid;
            String s = "";
            s += String.Format("Usage: {0} -attach PID [-o logName] [-t timeoutInMiliseconds]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -attach PID      \t- attaches to the target process\r\n";
            s += "  -o logName       \t- file to write the resulting profile log to\r\n";
            s += "  -t timeout       \t- max time in miliseconds to wait for the attach to succeed\r\n"; 
            s += "or\r\n";
            s += String.Format("{0} -detach PID\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -detach PID      \t- detaches from the target process (use -attach first)\r\n";
            s += "or\r\n";
            s += String.Format("{0} -dumpheap PID\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -dumpheap PID    \t- triggers a GC and dumps the GC heap in the target process (use -attach first)\r\n";
            s += "or\r\n";
            s += String.Format("{0} -o logName [-na] [-nc] -[np] [-target V4DesktopCLR|V4CoreCLR|V2DesktopCLR] -u URL\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -u URL\t- URL to profile (must go at the end)\r\n";
            s += "  -o logName\t- file to write the resulting profile log to\r\n";
            s += "  -na\t\t- don't record allocations\r\n";
            s += "  -nc\t\t- don't record calls\r\n";
            s += "  -np\t\t- start with profiling active off\r\n";
            s += "  -target V4DesktopCLR|V4CoreCLR|V2DesktopCLR\t- specify the target CLR runtime to be profiled (V4DesktopCLR is the default choice)\r\n";
            s += "or\r\n";
            s += String.Format("{0} [-o logName] [-na] [-nc] -[np] [-target V4DesktopCLR|V4CoreCLR|V2DesktopCLR] -p exeName [args]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -p exeName args\t- application to profile and its arguments (must go at the end)\r\n";
            s += "  -o logName\t- file to write the resulting profile log to\r\n";
            s += "  -na\t\t- don't record allocations\r\n";
            s += "  -nc\t\t- don't record calls\r\n";
            s += "  -np\t\t- start with profiling active off\r\n";
            s += "  -target V4DesktopCLR|V4CoreCLR|V2DesktopCLR\t- specify the target CLR runtime to be profiled (V4DesktopCLR is the default choice)\r\n";
            s += "or\r\n";
            s += String.Format("{0} -a -l logName [-b <start marker>] [-e <end marker>]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -a               \t - asks for an allocation report\r\n";
            s += "  -l logName       \t - names the input log file\r\n";
            s += "  -b <start marker>\t - only report allocations after this log file comment\r\n";
            s += "  -e <end marker>  \t - only report allocations before this log file comment\r\n";
            s += "or\r\n";
            s += String.Format("{0} -r -l logName [-b <start marker>] [-e <end marker>]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -r               \t - asks for a relocation report\r\n";
            s += "  -l logName       \t - names the input log file\r\n";
            s += "  -b <start marker>\t - only report relocations after this log file comment\r\n";
            s += "  -e <end marker>  \t - only report relocations before this log file comment\r\n";
            s += "or\r\n";
            s += String.Format("{0} -s -l logName [-b <start marker>] [-e <end marker>] [-t <time marker]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -s               \t - asks for a surviving objects report\r\n";
            s += "  -l logName       \t - names the input log file\r\n";
            s += "  -b <start marker>\t - only report objects allocated after this log file comment\r\n";
            s += "  -e <end marker>  \t - only report objects allocated before this log file comment\r\n";
            s += "  -t <time marker> \t - report survivors at this log file comment\r\n";
            s += "or\r\n";
            s += String.Format("{0} -[c]f -l logName [-b <start marker>] [-e <end marker>]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -[c]f            \t - asks for a [critical] finalizer report\r\n";
            s += "  -l logName       \t - names the input log file\r\n";
            s += "  -b <start marker>\t - only report finalizers queued after this log file comment\r\n";
            s += "  -e <end marker>  \t - only report finalizers queued before this log file comment\r\n";
            s += "or\r\n";
            s += String.Format("{0} -sd -l logName [-b <start marker>] [-e <end marker>]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -sd              \t - asks for a survivor difference report\r\n";
            s += "  -l logName       \t - names the input log file\r\n";
            s += "  -b <start marker>\t - first instant in time to gather survivors\r\n";
            s += "  -e <end marker>  \t - second instant in time to compare survivorship agains\r\n";
            s += "or\r\n";
            s += String.Format("{0} -h -l logName [-b <start marker>] [-e <end marker>]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -h               \t - asks for a heap dump report\r\n";
            s += "  -l logName       \t - names the input log file\r\n";
            s += "  -b <start marker>\t - only report heap dumps after this log file comment\r\n";
            s += "  -e <end marker>  \t - only report heap dumps before this log file comment\r\n";
            s += "                   \t - (giving only -b reports the first heap after the log file comment)\r\n";
            s += "or\r\n";
            s += String.Format("{0} -c -l logName\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "  -c               \t - asks for a list of the comments in the input log file\r\n";
            s += "or\r\n";
            s += String.Format("{0} -lr -l logName [-b <start marker>] [-e <end marker>]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "Where\r\n";
            s += "  -lr              \t - asks for a leak report\r\n";
            s += "  -l logName       \t - names the input log file\r\n";
            s += "  -b <start marker>\t - only report allocations after this log file comment\r\n";
            s += "  -e <end marker>  \t - only report allocations before this log file comment\r\n";
            s += "or\r\n";
            s += String.Format("{0} -diff [-lo OldlogName -ln NewlogName] [-ld DifflogName][-w]\r\n", Path.GetFileName(Application.ExecutablePath));
            s += "Where\r\n";
            s += "  -diff            \t- asks for a difference report\r\n";
            s += "  -lo logName      \t- old resulting profile log file\r\n";
            s += "  -ln logName      \t- new resulting profile log file\r\n";
            s += "  -ld logName      \t- diff result of old and new profile log file\r\n";
            s += "  -w               \t- run as window (otherwise textual report)\r\n";

            if (noUI)
            {
                Console.Write(s);
            }
            else
            {
                var helpForm = new HelpForm();
                helpForm.helpText.Text = s;
                helpForm.ShowDialog();
            }
            return;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private bool ProfiledProcessHasExited()
        {
            if (profiledProcess == null)
            {
                return true;
            }

            try
            {
                return profiledProcess.HasExited;
            }
            catch
            {
                return Process.GetProcessById(profiledProcess.Id) == null;
            }

        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] arguments)
        {
            consoleCtrl = new ConsoleCtrl();
            consoleCtrl.ControlEvent += new ConsoleCtrl.ControlEventHandler(ConsoleEventHandler);
            var f = new MainForm(arguments);

            if (f.exitProgram)
            {
                return;
            }

            if (!f.viewdiff && f.reportKind == ReportKind.NoReport && f.processFileName == null)
            {
                Application.Run(f);
            }
            else
            {
                try
                {
                    var _MgrForm = new ReportForm(f);
                    if (!f.noUI)
                    {
                        Application.Run(_MgrForm);
                    }
                }
                catch
                {
                    Application.Exit();
                }
            }
        }

        static ConsoleCtrl consoleCtrl;

        static bool ConsoleEventHandler(ConsoleCtrl.ConsoleEvent consoleEvent)
        {
            MainForm instance = MainForm.instance;
            if (instance == null || (instance.CheckProcessTerminate() && instance.CheckFileSave()))
            {
                Application.Exit();
            }

            return true;
        }

        private void ViewGraph(ReadLogResult logResult, string exeName, Graph.GraphType graphType)
        {
            string fileName = log.fileName;
            if (exeName != null)
            {
                fileName = exeName;
            }

            Graph graph = null;
            string title = "";
            switch (graphType)
            {
                case Graph.GraphType.CallGraph:
                    graph = logResult.callstackHistogram.BuildCallGraph(new FilterForm());
                    title = "Call Graph for: ";
                    break;

                case Graph.GraphType.AssemblyGraph:
                    graph = logResult.callstackHistogram.BuildAssemblyGraph(new FilterForm());
                    title = "Assembly Graph for: ";
                    break;

                case Graph.GraphType.AllocationGraph:
                    graph = logResult.allocatedHistogram.BuildAllocationGraph(new FilterForm());
                    title = "Allocation Graph for: ";
                    break;

                case Graph.GraphType.HeapGraph:
                    graph = logResult.objectGraph.BuildTypeGraph(new FilterForm());
                    title = "Heap Graph for: ";
                    break;

                case Graph.GraphType.FunctionGraph:
                    graph = logResult.functionList.BuildFunctionGraph(new FilterForm());
                    title = "Function Graph for: ";
                    break;

                case Graph.GraphType.ModuleGraph:
                    graph = logResult.functionList.BuildModuleGraph(new FilterForm());
                    title = "Module Graph for: ";
                    break;

                case Graph.GraphType.ClassGraph:
                    graph = logResult.functionList.BuildClassGraph(new FilterForm());
                    title = "Class Graph for: ";
                    break;

                default:
                    Debug.Assert(false);
                    break;
            }
            title += fileName + " " + commandLine;
            var graphViewForm = new GraphViewForm(graph, title);
            graphViewForm.Visible = true;
        }

        private void readLogFile(ReadNewLog log, ReadLogResult logResult, string exeName, Graph.GraphType graphType)
        {
            log.ReadFile(logFileStartOffset, logFileEndOffset, logResult);
            ViewGraph(logResult, exeName, graphType);
        }

        private void EnableDisableLaunchControls(bool enable)
        {
            // Buttons
            startApplicationButton.Enabled = 
                startWindowsStoreAppButton.Enabled =
                startURLButton.Enabled = 
                attachProcessButton.Enabled = 
                detachProcessButton.Enabled = 
                targetCLRVersioncomboBox.Enabled = 
                enable;

            // Menu items
            logFileOpenMenuItem.Enabled =
                profileApplicationMenuItem.Enabled =
                startURLMenuItem.Enabled =
                attachProcessMenuItem.Enabled =
                profileASP_NETmenuItem.Enabled =
                profileServiceMenuItem.Enabled =
                setCommandLineMenuItem.Enabled =
                    enable;
        }

        private void EnableDisableViewMenuItems()
        {
            if (lastLogResult == null)
            {
                viewAllocationGraphMenuItem.Enabled = false;
                viewCallGraphMenuItem.Enabled = false;
                viewHeapGraphMenuItem.Enabled = false;
                viewHistogramAllocatedMenuItem.Enabled = false;
                viewHistogramRelocatedMenuItem.Enabled = false;
                viewHistogramFinalizerMenuItem.Enabled = false;
                viewHistogramCriticalFinalizerMenuItem.Enabled = false;
                viewHistogramByAgeMenuItem.Enabled = false;
                viewObjectsByAddressMenuItem.Enabled = false;
                viewTimeLineMenuItem.Enabled = false;
                viewFunctionGraphMenuItem.Enabled = false;
                viewModuleGraphMenuItem.Enabled = false;
                viewClassGraphMenuItem.Enabled = false;
                viewCallTreeMenuItem.Enabled = false;
                viewAssemblyGraphMenuItem.Enabled = false;
                viewComparisonMenuItem.Enabled = false;
                viewSummaryMenuItem.Enabled = false;
            }
            else
            {
                viewAllocationGraphMenuItem.Enabled = !lastLogResult.allocatedHistogram.Empty;
                viewCallGraphMenuItem.Enabled = !lastLogResult.callstackHistogram.Empty;
                viewAssemblyGraphMenuItem.Enabled = !lastLogResult.callstackHistogram.Empty;
                viewHeapGraphMenuItem.Enabled = !lastLogResult.objectGraph.empty;
                viewHistogramAllocatedMenuItem.Enabled = !lastLogResult.allocatedHistogram.Empty;
                viewHistogramRelocatedMenuItem.Enabled = !lastLogResult.relocatedHistogram.Empty;
                viewHistogramFinalizerMenuItem.Enabled = !lastLogResult.finalizerHistogram.Empty;
                viewHistogramCriticalFinalizerMenuItem.Enabled = !lastLogResult.criticalFinalizerHistogram.Empty;
                viewHistogramByAgeMenuItem.Enabled = lastLogResult.liveObjectTable != null;
                viewObjectsByAddressMenuItem.Enabled = lastLogResult.liveObjectTable != null;
                viewTimeLineMenuItem.Enabled = lastLogResult.sampleObjectTable != null;
                viewFunctionGraphMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewModuleGraphMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewClassGraphMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewCallTreeMenuItem.Enabled = lastLogResult.hadCallInfo;
                viewComparisonMenuItem.Enabled = !lastLogResult.functionList.Empty;
                viewSummaryMenuItem.Enabled = true;
            }
            viewCommentsMenuItem.Enabled = (log != null && log.commentEventList.count != 0);
        }

        private void LoadLogFile(string logFileName)
        {
            if (logFileName.EndsWith(".exe",                        //The string to compare to the substring at the end of this instance
                                     true,                          //true to ignore case during the comparison; otherwise, false.
                                     CultureInfo.InvariantCulture   //An invariant culture is culture-insensitive.
                                     ))
            {
                ShowErrorMessage(logFileName +" is not a valid CLRProfiler log file name.");
                Environment.ExitCode = 1;
                exitProgram = true;
                return;
            }
            this.logFileName = logFileName;
            logFileStartOffset = 0;
            logFileEndOffset = long.MaxValue;

            processFileName = null;

            log = new ReadNewLog(logFileName);
            lastLogResult = null;
            ObjectGraph.cachedGraph = null;
            ReadLogResult readLogResult = GetLogResult();
            log.ReadFile(logFileStartOffset, logFileEndOffset, readLogResult);
            lastLogResult = readLogResult;
            Text = "Analyzing " + logFileName;
            EnableDisableViewMenuItems();
            viewSummaryMenuItem_Click(null, null);
        }

        private void fileOpenMenuItem_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            saveAsMenuItem.Enabled = false;

            openFileDialog.FileName = "*.log";
            openFileDialog.Filter = "Allocation Logs | *.log";
            if (openFileDialog.ShowDialog() == DialogResult.OK
                && openFileDialog.CheckFileExists)
            {
                LoadLogFile(openFileDialog.FileName);
            }
        }

        private void SaveFile()
        {
            string baseName = Path.GetFileNameWithoutExtension(processFileName);
            string fileName = Path.ChangeExtension(processFileName, ".log");
            int count = 0;
            while (File.Exists(fileName))
            {
                count++;
                fileName = string.Format("{0}_{1}.log", baseName, count);
            }
            saveFileDialog.FileName = fileName;
            saveFileDialog.Filter = "Allocation Logs | *.log";
            DialogResult retValue = saveFileDialog.ShowDialog();
            if (retValue == DialogResult.OK)
            {
                try
                {
                    if (File.Exists(saveFileDialog.FileName) && saveFileDialog.FileName != logFileName)
                    {
                        File.Delete(saveFileDialog.FileName);
                    }
                }
                catch
                {
                    MessageBox.Show(this, "Cannot delete existing file " + saveFileDialog.FileName, "Failure");
                    return;
                }
                try
                {
                    File.Move(logFileName, saveFileDialog.FileName);
                }
                catch
                {
                    MessageBox.Show(this, "Cannot rename log file " + logFileName + " to " + saveFileDialog.FileName, "Failure");
                    return;
                }
                if (!noUI)
                {
                    saveAsMenuItem.Enabled = false;
                }
                if (log != null)
                {
                    log.fileName = saveFileDialog.FileName;
                }
            }
        }

        private bool CheckFileSave()
        {
            if (saveAsMenuItem?.Enabled == true)
            {
                if (saveNever)
                {
                    try
                    {
                        File.Delete(logFileName);
                    }
                    catch (System.IO.IOException e)
                    {
                        MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error");
                    }
                }
                else
                {
                    var saveFileForm = new SaveFileForm();
                    saveFileForm.processFileNameLabel.Text = processFileName;
                    switch (saveFileForm.ShowDialog())
                    {
                        case DialogResult.Yes:
                            SaveFile();
                            break;

                        case DialogResult.No:
                            try
                            {
                                File.Delete(logFileName);
                            }
                            catch (System.IO.IOException e)
                            {
                                MessageBox.Show(e.Message + "\n" + e.StackTrace, "Error");
                            }
                            saveAsMenuItem.Enabled = false;
                            break;

                        case DialogResult.Cancel:
                            return false;

                        case DialogResult.Retry:
                            saveNever = true;
                            break;
                    }
                }
            }
            return true;
        }

        private bool CheckProcessTerminate()
        {
            if (killApplicationButton?.Enabled == true)
            {
                var killProcessForm = new KillProcessForm();
                killProcessForm.processFileNameLabel.Text = processFileName;
                switch (killProcessForm.ShowDialog())
                {
                    case DialogResult.Yes:
                        if (profiledProcess != null)
                        {
                            killApplicationButton_Click(null, null);
                            saveAsMenuItem.Enabled = true;
                        }
                        break;

                    case DialogResult.No:
                        ClearProfiledProcessInfo();
                        break;

                    case DialogResult.Cancel:
                        return false;
                }
            }
            return true;
        }

        private void exitMenuItem_Click(object sender, System.EventArgs e)
        {
            if (CheckProcessTerminate() && CheckFileSave())
            {
                Application.Exit();
            }
        }

        private void fontMenuItem_Click(object sender, System.EventArgs e)
        {
            if (fontDialog.ShowDialog() == DialogResult.OK)
            {
                font = fontDialog.Font;
            }
        }

        private void profileApplicationMenuItem_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            openFileDialog.FileName = "*.exe";
            openFileDialog.Filter = "Applications | *.exe";
            if (openFileDialog.ShowDialog() == DialogResult.OK
                && openFileDialog.CheckFileExists)
            {
                processFileName = openFileDialog.FileName;
                Text = "Profiling: " + processFileName + " " + commandLine;
                startApplicationButton.Text = "Start Application";
                killApplicationButton.Text = "Kill Application";
                serviceName = null;
                startApplicationButton_Click(null, null);
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private void StopIIS()
        {
            // stop IIS
            Text = "Stopping IIS ";
            var processStartInfo = new ProcessStartInfo("cmd.exe");
            if (Environment.OSVersion.Version.Major >= 6/*Vista*/)
            {
                processStartInfo.Arguments = "/c net stop was /y";
            }
            else
            {
                processStartInfo.Arguments = "/c net stop iisadmin /y";
            }

            var process = Process.Start(processStartInfo);
            while (!process.HasExited)
            {
                Text += ".";
                Thread.Sleep(100);
                Application.DoEvents();
            }
            if (process.ExitCode != 0)
            {
                Text += string.Format(" Error {0} occurred", process.ExitCode);
            }
            else
            {
                Text = "IIS stopped";
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private bool StartIIS()
        {
            Text = "Starting IIS ";
            var processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.Arguments = "/c net start w3svc";
            var process = Process.Start(processStartInfo);
            while (!process.HasExited)
            {
                Text += ".";
                Thread.Sleep(100);
                Application.DoEvents();
            }
            if (process.ExitCode != 0)
            {
                Text += string.Format(" Error {0} occurred", process.ExitCode);
                return false;
            }
            Text = "IIS running";
            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private void StopService(string serviceName, string stopCommand)
        {
            // stop service
            Text = "Stopping " + serviceName + " ";
            var processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.Arguments = "/c " + stopCommand;
            var process = Process.Start(processStartInfo);
            while (!process.HasExited)
            {
                Text += ".";
                Thread.Sleep(1000);
            }
            if (process.ExitCode != 0)
            {
                Text += string.Format(" Error {0} occurred", process.ExitCode);
            }
            else
            {
                Text = serviceName + " stopped";
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private Process StartService(string serviceName, string startCommand)
        {
            Text = "Starting " + serviceName + " ";
            var processStartInfo = new ProcessStartInfo("cmd.exe");
            processStartInfo.Arguments = "/c " + startCommand;
            var process = Process.Start(processStartInfo);
            return process;
        }

        private static unsafe int wcslen(char* s)
        {
            char* e;
            for (e = s; *e != '\0'; e++)
            {
                ;
            }

            return (int)(e - s);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private string[] GetServicesEnvironment()
        {
            Process[] servicesProcesses = Process.GetProcessesByName("services");
            if (servicesProcesses == null || servicesProcesses.Length != 1)
            {
                servicesProcesses = Process.GetProcessesByName("services.exe");
                if (servicesProcesses == null || servicesProcesses.Length != 1)
                {
                    return new string[0];
                }
            }
            Process servicesProcess = servicesProcesses[0];
            IntPtr processHandle = OpenProcess(0x20400, false, servicesProcess.Id);
            if (processHandle == IntPtr.Zero)
            {
                return new string[0];
            }

            IntPtr tokenHandle = IntPtr.Zero;
            if (!OpenProcessToken(processHandle, 0x20008, ref tokenHandle))
            {
                return new string[0];
            }

            IntPtr environmentPtr = IntPtr.Zero;
            if (!CreateEnvironmentBlock(out environmentPtr, tokenHandle, false))
            {
                return new String[0];
            }

            unsafe
            {
                string[] envStrings = null;
                // rather than duplicate the code that walks over the environment, 
                // we have this funny loop where the first iteration just counts the strings,
                // and the second iteration fills in the strings
                for (int i = 0; i < 2; i++)
                {
                    var env = (char*)environmentPtr.ToPointer();
                    int count = 0;
                    while (true)
                    {
                        int len = wcslen(env);
                        if (len == 0)
                        {
                            break;
                        }

                        if (envStrings != null)
                        {
                            envStrings[count] = new String(env);
                        }

                        count++;
                        env += len + 1;
                    }
                    if (envStrings == null)
                    {
                        envStrings = new string[count];
                    }
                }
                return envStrings;
            }
        }

        private string[] CombineEnvironmentVariables(string[] a, string[] b)
        {
            string[] c = new string[a.Length + b.Length];
            int i = 0;
            foreach (string s in a)
            {
                c[i++] = s;
            }

            foreach (string s in b)
            {
                c[i++] = s;
            }

            return c;
        }

        private Microsoft.Win32.RegistryKey GetServiceKey(string serviceName)
        {
            Microsoft.Win32.RegistryKey localMachine = Microsoft.Win32.Registry.LocalMachine;
            Microsoft.Win32.RegistryKey key = localMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Services\\" + serviceName, true);
            return key;
        }

        private void SetEnvironmentVariables(string serviceName, string[] environment)
        {
            Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
            key?.SetValue("Environment", environment);
        }

        private void DeleteEnvironmentVariables(string serviceName)
        {
            Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
            key?.DeleteValue("Environment");
        }

        private string EnvKey(string envVariable)
        {
            int index = envVariable.IndexOf('=');
            Debug.Assert(index >= 0);
            return envVariable.Substring(0, index);
        }

        private string EnvValue(string envVariable)
        {
            int index = envVariable.IndexOf('=');
            Debug.Assert(index >= 0);
            return envVariable.Substring(index + 1);
        }

        private Microsoft.Win32.RegistryKey GetAccountEnvironmentKey(string serviceAccountSid)
        {
            Microsoft.Win32.RegistryKey users = Microsoft.Win32.Registry.Users;
            return users.OpenSubKey(serviceAccountSid + @"\Environment", true);
        }

        private void SetAccountEnvironment(string serviceAccountSid, string[] profilerEnvironment)
        {
            Microsoft.Win32.RegistryKey key = GetAccountEnvironmentKey(serviceAccountSid);
            if (key != null)
            {
                foreach (string envVariable in profilerEnvironment)
                {
                    key.SetValue(EnvKey(envVariable), EnvValue(envVariable));
                }
            }
        }

        private void ResetAccountEnvironment(string serviceAccountSid, string[] profilerEnvironment)
        {
            Microsoft.Win32.RegistryKey key = GetAccountEnvironmentKey(serviceAccountSid);
            if (key != null)
            {
                foreach (string envVariable in profilerEnvironment)
                {
                    key.DeleteValue(EnvKey(envVariable));
                }
            }
        }

        private string CreateUsageString()
        {
            int index = 0;
            string[] usageStrings = new string[] { "none", "objects", "trace", "both" };
            if ((noUI && profileAllocations) || (!noUI && allocationsCheckBox.Checked))
            {
                index |= 1;
            }
            if ((noUI && profileCalls) || (!noUI && callsCheckBox.Checked))
            {
                index |= 2;
            }

            return usageStrings[index];
        }

        private string CreateInitialString()
        {
            int flags = 0;
            if ((noUI && profileAllocations && profilingActive) || (!noUI && allocationsCheckBox.Checked && profilingActiveCheckBox.Checked))
            {
                flags |= 1;
            }
            if ((noUI && profileCalls && profilingActive) || (!noUI && callsCheckBox.Checked && profilingActiveCheckBox.Checked))
            {
                flags |= 2;
            }
            return flags.ToString();
        }

        private string[] CreateProfilerEnvironment(string tempDir)
        {
            return new string[]
            { 
                "Cor_Enable_Profiling=0x1",
                "COR_PROFILER={8C29BC4E-1F57-461a-9B51-1200C32E6F1F}",
                "COR_PROFILER_PATH=" + getProfilerFullPath(),
                "OMV_SKIP=0",
                "OMV_FORMAT=v2",
                "OMV_STACK=" + (trackCallStacks ? "1" : "0"),
                "OMV_DynamicObjectTracking=0x1",
                "OMV_PATH=" + tempDir,
                "OMV_USAGE=" + CreateUsageString(),
                "OMV_FORCE_GC_ON_COMMENT=" + (gcOnLogFileComments ? "1" : "0"),
                "OMV_INITIAL_SETTING=" + CreateInitialString(),
                "OMV_TargetCLRVersion=" + (targetv2DesktopCLR() ? "v2" : "v4"),
                "OMV_WindowsStoreApp=" + (IsProfilingWindowsStoreApp() ? "1" : "0")
            };
        }

        private string GetServiceAccountName(string serviceName)
        {
            Microsoft.Win32.RegistryKey key = GetServiceKey(serviceName);
            if (key != null)
            {
                return key.GetValue("ObjectName") as string;
            }

            return null;
        }

        private string LookupAccountSid(string accountName)
        {
            int sidLen = 0;
            byte[] sid = new byte[sidLen];
            int domainNameLen = 0;
            int peUse;
            var domainName = new StringBuilder();
            LookupAccountName(Environment.MachineName, accountName, sid, ref sidLen, domainName, ref domainNameLen, out peUse);

            sid = new byte[sidLen];
            domainName = new StringBuilder(domainNameLen);
            string stringSid = null;
            if (LookupAccountName(Environment.MachineName, accountName, sid, ref sidLen, domainName, ref domainNameLen, out peUse))
            {
                IntPtr stringSidPtr;
                if (ConvertSidToStringSidW(sid, out stringSidPtr))
                {
                    try
                    {
                        stringSid = Marshal.PtrToStringUni(stringSidPtr);
                    }
                    finally
                    {
                        LocalFree(stringSidPtr);
                    }
                }
            }
            return stringSid;
        }

        private string[] ReplaceTempDir(string[] env, string newTempDir)
        {
            for (int i = 0; i < env.Length; i++)
            {
                if (env[i].StartsWith("TEMP="))
                {
                    env[i] = "TEMP=" + newTempDir;
                }
                else if (env[i].StartsWith("TMP="))
                {
                    env[i] = "TMP=" + newTempDir;
                }
            }
            return env;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private void ProfileService()
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            if (targetv2DesktopCLR())
            {
                RegisterDLL.Register();  // Register profilerOBJ.dll for v2 CLR, which doesn't support registry free activation
            }

            StopService(serviceName, serviceStopCommand);

            string logDir = GetLogDir();
            string[] profilerEnvironment = CreateProfilerEnvironment(logDir);

            // set environment variables

            // this is a bit intricate - if the service is running as LocalSystem, we need to set the environment
            // variables in the registry for the service, otherwise it's better to temporarily set it for the account,
            // assuming we can find out the account SID
            string serviceAccountName = GetServiceAccountName(serviceName);
            if (serviceAccountName.StartsWith(@".\"))
            {
                serviceAccountName = Environment.MachineName + serviceAccountName.Substring(1);
            }

            if (serviceAccountName != null && serviceAccountName != "LocalSystem")
            {
                serviceAccountSid = LookupAccountSid(serviceAccountName);
            }
            if (serviceAccountSid != null)
            {
                SetAccountEnvironment(serviceAccountSid, profilerEnvironment);
            }
            else
            {
                string[] baseEnvironment = GetServicesEnvironment();
                baseEnvironment = ReplaceTempDir(baseEnvironment, GetLogDir());
                string[] combinedEnvironment = CombineEnvironmentVariables(baseEnvironment, profilerEnvironment);
                SetEnvironmentVariables(serviceName, combinedEnvironment);
            }

            Process cmdProcess = StartService(serviceName, serviceStartCommand);

            // wait for service to start up and connect
            Text = string.Format("Waiting for {0} to start up", serviceName);

            Thread.Sleep(1000);
            int pid = WaitForProcessToConnect(logDir, "Waiting for service to start common language runtime");
            if (pid > 0)
            {
                profiledProcess = Process.GetProcessById(pid);

                Text = "Profiling: " + serviceName;
                startApplicationButton.Text = "Start " + serviceName;
                killApplicationButton.Text = "Kill " + serviceName;
                processFileName = serviceName;
            }

            /* Delete the environment variables as early as possible, so that even if CLRProfiler crashes, the user's machine
             * won't be screwed up.
             * */

            if (serviceAccountSid != null)
            {
                ResetAccountEnvironment(serviceAccountSid, profilerEnvironment);
            }
            else
            {
                DeleteEnvironmentVariables(serviceName);
            }
        }

        private string GetASP_NETaccountName()
        {
            try
            {
                var machineConfig = new XmlDocument();
                string runtimePath = RuntimeEnvironment.GetRuntimeDirectory();
                string configPath = Path.Combine(runtimePath, @"CONFIG\machine.config");
                machineConfig.Load(configPath);
                XmlNodeList elemList = machineConfig.GetElementsByTagName("processModel");
                for (int i = 0; i < elemList.Count; i++)
                {
                    XmlAttributeCollection attributes = elemList[i].Attributes;
                    XmlAttribute userNameAttribute = attributes["userName"];
                    if (userNameAttribute != null)
                    {
                        string userName = userNameAttribute.InnerText;
                        if (userName == "machine")
                        {
                            return "ASPNET";
                        }
                        else if (userName == "SYSTEM")
                        {
                            return null;
                        }
                        else
                        {
                            return userName;
                        }
                    }
                }
            }
            catch
            {
                // swallow all exceptions here
            }
            return "ASPNET";
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private void profileASP_NETmenuItem_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            if (targetv2DesktopCLR())
            {
                RegisterDLL.Register();  // Register profilerOBJ.dll for v2 CLR, which doesn't support registry free activation
            }

            StopIIS();

            // set environment variables

            string logDir = GetLogDir();
            string[] profilerEnvironment = CreateProfilerEnvironment(logDir);

            string[] baseEnvironment = GetServicesEnvironment();
            baseEnvironment = ReplaceTempDir(baseEnvironment, GetLogDir());
            string[] combinedEnvironment = CombineEnvironmentVariables(baseEnvironment, profilerEnvironment);
            SetEnvironmentVariables("IISADMIN", combinedEnvironment);
            SetEnvironmentVariables("W3SVC", combinedEnvironment);
            SetEnvironmentVariables("WAS", combinedEnvironment);

            string asp_netAccountName = GetASP_NETaccountName();
            string asp_netAccountSid = null;
            if (asp_netAccountName != null)
            {
                asp_netAccountSid = LookupAccountSid(asp_netAccountName);
                if (asp_netAccountSid != null)
                {
                    SetAccountEnvironment(asp_netAccountSid, profilerEnvironment);
                }
            }

            if (StartIIS())
            {
                // wait for worker process to start up and connect
                Text = "Waiting for ASP.NET worker process to start up";

                Thread.Sleep(1000);
                int pid = WaitForProcessToConnect(logDir, "Waiting for ASP.NET to start common language runtime - this is the time to load your test page");
                if ( pid > 0)
                {
                    profiledProcess = Process.GetProcessById(pid);

                    Text = "Profiling: ASP.NET";
                    startApplicationButton.Text = "Start ASP.NET";
                    killApplicationButton.Text = "Kill ASP.NET";
                    processFileName = "ASP.NET";
                }
            }

            /* Delete the environment variables as early as possible, so that even if CLRProfiler crashes, the user's machine
             * won't be screwed up.
             * */
            DeleteEnvironmentVariables("IISADMIN");
            DeleteEnvironmentVariables("W3SVC");
            DeleteEnvironmentVariables("WAS");

            if (asp_netAccountSid != null)
            {
                ResetAccountEnvironment(asp_netAccountSid, profilerEnvironment);
            }

            serviceName = null;
        }

        [DllImport("Kernel32.dll", CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateNamedPipe(
            string lpName,         // pointer to pipe name
            uint dwOpenMode,       // pipe open mode
            uint dwPipeMode,       // pipe-specific modes
            uint nMaxInstances,    // maximum number of instances
            uint nOutBufferSize,   // output buffer size, in bytes
            uint nInBufferSize,    // input buffer size, in bytes
            uint nDefaultTimeOut,  // time-out time, in milliseconds
            ref SECURITY_ATTRIBUTES lpSecurityAttributes  // pointer to security attributes
            );

        [DllImport("Kernel32.dll")]
        private static extern IntPtr OpenProcess(
            uint dwDesiredAccess,  // access flag
            bool bInheritHandle,    // handle inheritance option
            int dwProcessId       // process identifier
            );

        [DllImport("Advapi32.dll")]
        private static extern bool OpenProcessToken(
            IntPtr ProcessHandle,
            uint DesiredAccess,
            ref IntPtr TokenHandle
            );

        [DllImport("UserEnv.dll")]
        private static extern bool CreateEnvironmentBlock(
                out IntPtr lpEnvironment,
                IntPtr hToken,
                bool bInherit);

        [DllImport("UserEnv.dll")]
        private static extern bool DestroyEnvironmentBlock(
                IntPtr lpEnvironment);

        [DllImport("Advapi32.dll")]
        private static extern bool ConvertStringSecurityDescriptorToSecurityDescriptor(
            string StringSecurityDescriptor,
            uint StringSDRevision,
            out IntPtr SecurityDescriptor,
            IntPtr SecurityDescriptorSize
            );

        [DllImport("Kernel32.dll")]
        private static extern bool LocalFree(IntPtr ptr);

        [DllImport("Advapi32.dll")]
        private static extern bool ConvertSidToStringSidW(byte[] sid, out IntPtr stringSid);

        [DllImport("Advapi32.dll")]
        private static extern bool LookupAccountName(string machineName, string accountName, byte[] sid,
                                 ref int sidLen, StringBuilder domainName, ref int domainNameLen, out int peUse);

        [DllImport("Kernel32.dll")]
        private static extern bool ConnectNamedPipe(
            SafeFileHandle hNamedPipe,  // handle to named pipe to connect
            IntPtr lpOverlapped         // pointer to overlapped structure
            );

        [DllImport("Kernel32.dll")]
        private static extern bool DisconnectNamedPipe(
            SafeFileHandle hNamedPipe   // handle to named pipe
            );

        [DllImport("Kernel32.dll")]
        private static extern int GetLastError();

        [DllImport("Kernel32.dll")]
        private static extern bool ReadFile(
            IntPtr hFile,                // handle of file to read
            byte[] lpBuffer,             // pointer to buffer that receives data
            uint nNumberOfBytesToRead,  // number of bytes to read
            out uint lpNumberOfBytesRead, // pointer to number of bytes read
            IntPtr lpOverlapped    // pointer to structure for data
            );

        [DllImport("Kernel32.dll")]
        private static extern int IsWow64Process(IntPtr process, out int wow64Process);

        private bool CreatePipe(string pipeName, bool blockingPipe, ref SafeFileHandle pipeHandle, ref FileStream pipe)
        {
            SECURITY_ATTRIBUTES sa;
            sa.nLength = 12;
            sa.bInheritHandle = 0;
            if (!ConvertStringSecurityDescriptorToSecurityDescriptor("D: (A;OICI;GRGW;;;AU)", 1, out sa.lpSecurityDescriptor, IntPtr.Zero))
            {
                return false;
            }

            uint flags = 4 | 2 | 0;

            if (!blockingPipe)
            {
                flags |= 1;
            }

            pipeHandle = CreateNamedPipe(pipeName, 3, flags, 1, 512, 512, 1000, ref sa);
            LocalFree(sa.lpSecurityDescriptor);
            if (pipeHandle.IsInvalid)
            {
                return false;
            }

            pipe = new FileStream(pipeHandle, FileAccess.ReadWrite, 512, false);
            return true;
        }

        private void ClosePipe(ref SafeFileHandle pipeHandle, ref FileStream pipe)
        {
            pipe.Close();
            pipe = null;
            pipeHandle = null;
        }

        private void InitWindowsStoreAppLogDirectory(string acFolderPath)
        {
            // Profiler will need to write under the AppContainer directory that
            // the Windows Store app is given access to.  Create a CLRProfiler subdirectory
            // under there for the profiler to use.
            logDirectory = acFolderPath + "\\CLRProfiler";
            if (!Directory.Exists(logDirectory))
            {
                Directory.CreateDirectory(logDirectory);
            }
        }
        
        private NamedManualResetEvent CreateEvent(string baseName, int pid, bool createEventHandle)
        {
            string eventName = string.Format("{0}{1}_{2:x8}", IsProfilingWindowsStoreApp() ? windowsStoreAppProfileeInfo.windowsStoreAppEventPrefix : "Global\\", baseName, pid);
            return new NamedManualResetEvent(eventName, false, createEventHandle);
        }

        private void CreateEvents(int pid)
        {
            try
            {
                loggingActiveEvent = CreateEvent("OMV_TriggerObjects", pid, true);
                loggingActiveCompletedEvent = CreateEvent("OMV_TriggerObjects_Completed", pid, true);
                forceGcEvent = CreateEvent("OMV_ForceGC", pid, true);
                forceGcCompletedEvent = CreateEvent("OMV_ForceGC_Completed", pid, true);
                callGraphActiveEvent = CreateEvent("OMV_Callgraph", pid, true);
                callGraphActiveCompletedEvent = CreateEvent("OMV_Callgraph_Completed", pid, true);
                detachEvent = CreateEvent("OMV_Detach", pid, true);
            }
            catch
            {
                MessageBox.Show("Could not create events - in case you are profiling a service, " +
                    "start the profiler BEFORE starting the service");
                throw;
            }
        }

        private void ClearEvents()
        {
            loggingActiveEvent.Dispose();
            loggingActiveEvent = null;
            loggingActiveCompletedEvent.Dispose();
            loggingActiveCompletedEvent = null;
            forceGcEvent.Dispose();
            forceGcEvent = null;
            forceGcCompletedEvent.Dispose();
            forceGcCompletedEvent = null;
            callGraphActiveEvent.Dispose();
            callGraphActiveEvent = null;
            callGraphActiveCompletedEvent.Dispose();
            callGraphActiveCompletedEvent = null;
            detachEvent.Dispose();
            detachEvent = null;
        }

        private string GetLogDir()
        {
            if (logDirectory != null)
            {
                return logDirectory;
            }

            string tempDir = null;
            string winDir = Environment.GetEnvironmentVariable("WINDIR");
            if (winDir != null)
            {
                tempDir = winDir + @"\TEMP";
                if (!Directory.Exists(tempDir))
                {
                    tempDir = null;
                }
            }
            if (tempDir == null)
            {
                tempDir = Environment.GetEnvironmentVariable("TEMP");
                if (tempDir == null)
                {
                    tempDir = Environment.GetEnvironmentVariable("TMP") ?? @"C:\TEMP";
                }
            }
            return tempDir;
        }

        private string GetLogFullPath(int pid)
        {
            return GetLogDir() + "\\" + getLogFileName(pid);
        }

        private void ClearProfiledProcessInfo()
        {
            profiledProcess = null;
            profilerConnected = false;
            if (WindowsStoreAppHelperWrapper.IsWindowsStoreAppSupported() && IsProfilingWindowsStoreApp())
            {
                WindowsStoreAppHelperWrapper.DisableDebuggingForPackage(windowsStoreAppProfileeInfo.packageFullName);
            }

            windowsStoreAppProfileeInfo = null;
        }

        // Checks if bitness of CLRProfiler.exe matches bitness of specified process.
        // If we can conclusively prove the bitnesses are different, then display an
        // error message and return false.  If the bitnesses are the same (or we failed
        // trying to determine that), just optimistically return true.
        private bool VerifyCorrectBitness(Process process)
        {
            if (process == null)
            {
                return true;
            }

            if (!Environment.Is64BitOperatingSystem)
            {
                // On 32-bit OS's everyone has the same bitness
                return true;
            }

            int areYouWow;
            if (IsWow64Process(process.Handle, out areYouWow) == 0)
            {
                return true;
            }

            bool areYou64 = (areYouWow == 0);

            bool amI64 = Environment.Is64BitProcess;

            if (amI64 == areYou64)
            {
                return true;
            }

            ShowErrorMessage(
                string.Format(
                    "You are trying to profile process ID {0} which is running as {1} bits, but CLRProfiler is currently running as {2} bits.  Please rerun the {1} bit version of CLRProfiler and try again.",
                    process.Id,
                    areYou64 ? "64" : "32",
                    amI64 ? "64" : "32"));

            return false;
        }

        //FIXME
        private int WaitForWindowsStoreAppProcessToConnect(uint pid, string text)
        {
            return WaitForWindowsStoreAppProcessToConnect(pid, text, false);
        }

        private int WaitForWindowsStoreAppProcessToConnect(uint pid, string text, bool attachMode)
        {
            return WaitForWindowsStoreAppProcessToConnect(pid, text, attachMode, 0);
        }

        private int WaitForWindowsStoreAppProcessToConnect(uint pid, string text, bool attachMode, uint result)
        {
            if (!VerifyCorrectBitness(Process.GetProcessById((int) pid)))
            {
                return -1;
            }

            WaitingForConnectionForm waitingForConnectionForm = null;

            //Do not show the text in attachmode 
            if (attachMode == false)
            {
                if (noUI)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    if (waitingForConnectionForm == null)
                    {
                        waitingForConnectionForm = new WaitingForConnectionForm();
                    }

                    waitingForConnectionForm.setMessage(text);
                    waitingForConnectionForm.Visible = true;
                }
            }

            string fileName = getLogFileName((int)pid);
            string logFilePath = GetLogDir() + "\\" + fileName;

            // When log file has been created, that's our sign that profilerObj.dll is up and running
            while (!File.Exists(logFilePath))
            {
                Application.DoEvents();
                if (!noUI)
                {
                    if (waitingForConnectionForm != null && waitingForConnectionForm.DialogResult == DialogResult.Cancel)
                    {
                        waitingForConnectionForm.Close();
                        return -1;
                    }
                }
                Thread.Sleep(100);
            }

            if (waitingForConnectionForm != null)
            {
                waitingForConnectionForm.Visible = false;
            }

            CreateEvents((int)pid);
            logFileName = logFilePath;
            log = new ReadNewLog(logFileName);

            if (noUI)
            {
                Console.WriteLine("CLRProfiler is loaded in the target process.");
            }
            else
            {
                EnableDisableViewMenuItems();
                EnableDisableLaunchControls(false);
                if (!allocationsCheckBox.Checked && callsCheckBox.Checked)
                {
                    showHeapButton.Enabled = false;
                }
                else
                {
                    showHeapButton.Enabled = true;
                }

                killApplicationButton.Enabled = true;
            }
            logFileStartOffset = 0;
            logFileEndOffset = long.MaxValue;
            profilerConnected = true;

            return (int) pid;
        }

        //FIXME
        private int WaitForProcessToConnect(string tempDir, string text)
        {
            return WaitForProcessToConnect(tempDir, text, false);
        }

        private int WaitForProcessToConnect(string tempDir, string text, bool attachMode)
        {
            return WaitForProcessToConnect(tempDir, text, attachMode, 0);
        }

        private int WaitForProcessToConnect(string tempDir, string text, bool attachMode, uint result)
        {
            bool fProfiledProcessInitialized = profiledProcess != null;
            if (fProfiledProcessInitialized)
            {
                if (!VerifyCorrectBitness(profiledProcess))
                {
                    return -1;
                }
            }


            ConnectNamedPipe(handshakingPipeHandle, IntPtr.Zero);
            ConnectNamedPipe(loggingPipeHandle, IntPtr.Zero);

            int pid = 0;
            byte[] handshakingBuffer = new byte[9];
            int handshakingReadBytes = 0;

            // IMPORTANT: maxloggingBufferSize must match bufferSize defined in ProfilerCallback.cpp.
            const int maxloggingBufferSize = 512;
            byte[] loggingBuffer = new byte[maxloggingBufferSize];
            int loggingReadBytes = 0;
            WaitingForConnectionForm waitingForConnectionForm = null;
            int beginTickCount = Environment.TickCount;

            //Do not show the text in attachmode 
            if (attachMode == false)
            {
                if (noUI)
                {
                    Console.WriteLine(text);
                }
                else
                {
                    if (waitingForConnectionForm == null)
                    {
                        waitingForConnectionForm = new WaitingForConnectionForm();
                    }

                    waitingForConnectionForm.setMessage(text);
                    waitingForConnectionForm.Visible = true;
                }
            }


            // loop reading two pipes,
            // until   
            //  (1)successfully connected 
            //  (2)User canceled
            //  (3)attach failed
            //  (4)target process exited
            while (true)
            {
                #region handshaking
                //(1)succeeded
                try
                {
                    handshakingReadBytes += handshakingPipe.Read(handshakingBuffer, handshakingReadBytes, 9 - handshakingReadBytes);
                }
                catch (System.IO.IOException)
                {
                }
                
                //Read 9 bytes from handshaking pipe
                //means the profielr was initialized successfully
                if (handshakingReadBytes == 9)
                {
                    break;
                }

                Application.DoEvents();
                //  (2)User canceled
                if (!noUI)
                {
                    if (waitingForConnectionForm != null && waitingForConnectionForm.DialogResult == DialogResult.Cancel)
                    {
                        pid = -1;
                        break;
                    }
                }
                #endregion handshaking
                #region logging
                //  (3)attach failed
                //  (3.1) read logging message
                //  (3.2) break if attach failed.

                //  (3.1) read logging message
                try
                {
                    loggingReadBytes += loggingPipe.Read(loggingBuffer, loggingReadBytes, maxloggingBufferSize - loggingReadBytes);
                }
                catch (System.IO.IOException)
                {
                }

                if (loggingReadBytes == maxloggingBufferSize)
                {
                    char[] charBuffer = new char[loggingReadBytes];
                    for (int i = 0; i < loggingReadBytes; i++)
                    {
                        charBuffer[i] = Convert.ToChar(loggingBuffer[i]);
                    }

                    string message = new String(charBuffer, 0, loggingReadBytes);

                    if (attachMode == false && noUI == false)
                    {
                        waitingForConnectionForm.addMessage(message);
                    }
                    else
                    {
                        ShowErrorMessage(message);
                    }

                    loggingReadBytes = 0;

                    while (true)
                    {
                        try
                        {
                            if (loggingPipe.Read(loggingBuffer, 0, 1) == 0)
                            {
                                DisconnectNamedPipe(loggingPipeHandle);
                                ConnectNamedPipe(loggingPipeHandle, IntPtr.Zero);
                                break;
                            }
                        }
                        catch (System.IO.IOException)
                        {
                            DisconnectNamedPipe(loggingPipeHandle);
                            ConnectNamedPipe(loggingPipeHandle, IntPtr.Zero);
                            break;
                        }
                    }
                }
                //  (3.2) break if attach failed.
                if (attachMode == true && result != 0)
                {
                    pid = -1;
                    break;
                }
                #endregion logging
                //  (4)target process exited
                if ((fProfiledProcessInitialized && profiledProcess == null) || (profiledProcess != null && ProfiledProcessHasExited()))
                {
                    pid = -1;
                    break;
                }
                Thread.Sleep(100);
            }

            if (waitingForConnectionForm != null)
            {
                waitingForConnectionForm.Visible = false;
            }

            if (pid == -1)
            {
                return pid;
            }

            if (handshakingReadBytes == 9)
            {
                char[] charBuffer = new char[9];
                for (int i = 0; i < handshakingBuffer.Length; i++)
                {
                    charBuffer[i] = Convert.ToChar(handshakingBuffer[i]);
                }

                pid = Int32.Parse(new String(charBuffer, 0, 8), NumberStyles.HexNumber);

                CreateEvents(pid);

                string fileName = getLogFileName(pid);
                byte[] fileNameBuffer = new Byte[fileName.Length + 1];
                for (int i = 0; i < fileName.Length; i++)
                {
                    fileNameBuffer[i] = (byte)fileName[i];
                }

                fileNameBuffer[fileName.Length] = 0;
                handshakingPipe.Write(fileNameBuffer, 0, fileNameBuffer.Length);
                handshakingPipe.Flush();
                logFileName = tempDir + "\\" + fileName;
                log = new ReadNewLog(logFileName);
                lastLogResult = null;
                ObjectGraph.cachedGraph = null;
                while (true)
                {
                    try
                    {
                        if (handshakingPipe.Read(handshakingBuffer, 0, 1) == 0) // && GetLastError() == 109/*ERROR_BROKEN_PIPE*/)
                        {
                            DisconnectNamedPipe(handshakingPipeHandle);
                            ConnectNamedPipe(handshakingPipeHandle, IntPtr.Zero);
                            break;
                        }
                    }
                    catch (System.IO.IOException)
                    {
                        DisconnectNamedPipe(handshakingPipeHandle);
                        ConnectNamedPipe(handshakingPipeHandle, IntPtr.Zero);
                        break;
                    }
                }
            }
            else
            {
                string error = string.Format("Error {0} occurred", GetLastError());
                ShowErrorMessage(error);
            }

            if (noUI)
            {
                Console.WriteLine("CLRProfiler is loaded in the target process.");
            }
            else
            {
                EnableDisableViewMenuItems();
                EnableDisableLaunchControls(false);

                if (!allocationsCheckBox.Checked && callsCheckBox.Checked)
                {
                    showHeapButton.Enabled = false;
                }
                else
                {
                    showHeapButton.Enabled = true;
                }

                killApplicationButton.Enabled = true;
                detachProcessMenuItem.Enabled = false;
            }
            logFileStartOffset = 0;
            logFileEndOffset = long.MaxValue;
            profilerConnected = true;

            return pid;
        }

        private void startWindowsStoreAppButton_Click(object sender, EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            // Win 8 check
            WindowsStoreAppHelperWrapper.Init();
            if (!WindowsStoreAppHelperWrapper.IsWindowsStoreAppSupported())
            {
                // Error message was already displayed by WindowsStoreAppHelperWrapper
                startWindowsStoreAppButton.Enabled = false;
                return;
            }

            if (WindowsStoreAppHelperWrapper.IsRunningElevated())
            {
                MessageBox.Show("CLRProfiler cannot launch a Windows Store app when running elevated.  Please rerun CLRProfiler non-elevated and try again.");
                return;
            }

            // Ensure WindowsStoreApp packages will have access to CLRProfiler binaries
            if (!WindowsStoreAppHelperWrapper.IsWindowsStoreAppAccessEnabledForProfiler(Path.GetDirectoryName(Application.ExecutablePath)))
            {
                return;
            }

            // User picks the package

            var windowsStoreAppAppChooserForm = new WindowsStoreAppChooserForm();
            DialogResult result = windowsStoreAppAppChooserForm.ShowDialog();
            if (result == DialogResult.Cancel)
            {
                return;
            }

            string packageFullName = windowsStoreAppAppChooserForm.SelectedPackageFullName;
            string appUserModelId = windowsStoreAppAppChooserForm.SelectedAppUserModelId;
            string acSidString = windowsStoreAppAppChooserForm.SelectedAcSidString;

            windowsStoreAppProfileeInfo = new WindowsStoreAppProfileeInfo(packageFullName, acSidString);
            InitWindowsStoreAppLogDirectory(windowsStoreAppAppChooserForm.SelectedPackageTempDir);

            // Launch the WindowsStoreApp with the right environment variables
            processFileName = windowsStoreAppAppChooserForm.SelectedProcessFileName;
            uint pid;
            WindowsStoreAppHelperWrapper.SpawnWindowsStoreAppProcess(
                packageFullName,
                appUserModelId,
                "",         // App args
                CreateProfilerEnvironment(logDirectory),
                out pid);

            // There was an error spawning the WindowsStoreApp process; an error message should
            // already have been displayed.
            if ((int)pid == -1)
            {
                return;
            }

            profiledProcess = Process.GetProcessById((int) pid);

            if (WaitForWindowsStoreAppProcessToConnect(pid, "Waiting for Windows Store application to start common language runtime") <= 0)
            {
                ClearProfiledProcessInfo();
            }
        }


        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private void startApplicationButton_Click(object sender, System.EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            if (processFileName == null)
            {
                profileApplicationMenuItem_Click(null, null);
                return;
            }
            else if (processFileName == "ASP.NET")
            {
                profileASP_NETmenuItem_Click(null, null);
                return;
            }
            else if (serviceName != null)
            {
                ProfileService();
                return;
            }

            if (targetv2DesktopCLR())
            {
                RegisterDLL.Register();  // Register profilerOBJ.dll for v2 CLR, which doesn't support registry free activation
            }

            if (processFileName == null)
            {
                return;
            }

            if (profiledProcess == null || ProfiledProcessHasExited())
            {
                var processStartInfo = new ProcessStartInfo(processFileName);
                if (targetv4CoreCLR())
                {
                    processStartInfo.EnvironmentVariables["CoreCLR_Enable_Profiling"] = "0x1";
                    processStartInfo.EnvironmentVariables["CORECLR_PROFILER"] = "{8C29BC4E-1F57-461a-9B51-1200C32E6F1F}";
                    processStartInfo.EnvironmentVariables["CORECLR_PROFILER_PATH"] = getProfilerFullPath();
                }
                else
                {
                    processStartInfo.EnvironmentVariables["Cor_Enable_Profiling"] = "0x1";
                    processStartInfo.EnvironmentVariables["COR_PROFILER"] = "{8C29BC4E-1F57-461a-9B51-1200C32E6F1F}";
                    processStartInfo.EnvironmentVariables["COR_PROFILER_PATH"] = getProfilerFullPath();
                }

                processStartInfo.EnvironmentVariables["OMV_USAGE"] = CreateUsageString();
                processStartInfo.EnvironmentVariables["OMV_SKIP"] = "0";
                processStartInfo.EnvironmentVariables["OMV_PATH"] = GetLogDir();
                processStartInfo.EnvironmentVariables["OMV_STACK"] = trackCallStacks ? "1" : "0";
                processStartInfo.EnvironmentVariables["OMV_FORMAT"] = "v2";
                processStartInfo.EnvironmentVariables["OMV_DynamicObjectTracking"] = "0x1";
                processStartInfo.EnvironmentVariables["OMV_FORCE_GC_ON_COMMENT"] = gcOnLogFileComments ? "1" : "0";
                processStartInfo.EnvironmentVariables["OMV_INITIAL_SETTING"] = CreateInitialString();
                processStartInfo.EnvironmentVariables["OMV_TargetCLRVersion"] = targetv2DesktopCLR() ? "v2" : "v4";


                if (commandLine != null)
                {
                    processStartInfo.Arguments = commandLine;
                }

                if (workingDirectory != null)
                {
                    processStartInfo.WorkingDirectory = workingDirectory;
                }

                processStartInfo.UseShellExecute = false;

                profiledProcess = Process.Start(processStartInfo);

                if (WaitForProcessToConnect(GetLogDir(), "Waiting for application to start common language runtime") <= 0)
                {
                    ClearProfiledProcessInfo();
                }
            }
        }

        private ReadLogResult GetLogResult()
        {
            ReadLogResult readLogResult = lastLogResult ?? new ReadLogResult();
            readLogResult.liveObjectTable = new LiveObjectTable(log);
            readLogResult.sampleObjectTable = new SampleObjectTable(log);
            readLogResult.allocatedHistogram = new Histogram(log);
            readLogResult.callstackHistogram = new Histogram(log);
            readLogResult.relocatedHistogram = new Histogram(log);
            readLogResult.finalizerHistogram = new Histogram(log);
            readLogResult.criticalFinalizerHistogram = new Histogram(log);
            readLogResult.createdHandlesHistogram = new Histogram(log);
            readLogResult.destroyedHandlesHistogram = new Histogram(log);
            if (readLogResult.objectGraph != null)
            {
                readLogResult.objectGraph.Neuter();
            }

            readLogResult.objectGraph = new ObjectGraph(log, 0);
            readLogResult.functionList = new FunctionList(log);
            readLogResult.hadCallInfo = readLogResult.hadAllocInfo = false;
            readLogResult.handleHash = new Dictionary<ulong, HandleInfo>();

            // We may just have turned a lot of data into garbage - let's try to reclaim the memory
            GC.Collect();

            return readLogResult;
        }

        private void ToggleEvent(NamedManualResetEvent toggleEvent, NamedManualResetEvent toggleEventCompleted)
        {
            if (profiledProcess != null && !ProfiledProcessHasExited() )
            {
                if (toggleEvent != null)
                {
                    toggleEvent.Set();
                    if (toggleEventCompleted.Wait(10 * 1000))
                    {
                        toggleEventCompleted.Reset();
                    }
                    else
                    {
                        MessageBox.Show("There was no response from the application");
                    }
                }
            }
        }

        private void profilingActiveCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            if (allocationsCheckBox.Checked)
            {
                ToggleEvent(loggingActiveEvent, loggingActiveCompletedEvent);
            }
            if (callsCheckBox.Checked)
            {
                ToggleEvent(callGraphActiveEvent, callGraphActiveCompletedEvent);
            }

            // disable check boxes if profiling is turned off
            if (profilingActiveCheckBox.Checked)
            {
                allocationsCheckBox.Enabled = callsCheckBox.Enabled = targetCLRVersioncomboBox.Enabled = true;
            }
            else
            {
                attachProcessButton.Enabled = allocationsCheckBox.Enabled = callsCheckBox.Enabled = targetCLRVersioncomboBox.Enabled = false;
            }

            enableDisableAttachProcessButtonAndMenuItem();
            detachProcessButton.Enabled = false;

            if (profiledProcess != null && !ProfiledProcessHasExited() && logFileName != null)
            {
                if (!File.Exists(logFileName))
                {
                    ShowErrorMessage(string.Format("Can not find log file '{0}'.", logFileName));
                    return;
                }
                Stream s = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                long offset = s.Length;
                s.Close();
                if (profilingActiveCheckBox.Checked)
                {
                    logFileStartOffset = offset;
                }
                else
                {
                    logFileEndOffset = offset;
                    if (logFileStartOffset >= logFileEndOffset && profiledProcess != null && !ProfiledProcessHasExited())
                    {
                        MessageBox.Show("No new data found", "",
                            MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    }
                }
                if (!profilingActiveCheckBox.Checked && logFileStartOffset < logFileEndOffset && logFileName != null)
                {
                    ReadLogResult readLogResult = GetLogResult();
                    log.ReadFile(logFileStartOffset, logFileEndOffset, readLogResult);
                    lastLogResult = readLogResult;
                    EnableDisableViewMenuItems();
                    viewSummaryMenuItem_Click(null, null);
                }
            }
        }

        private void allocationsCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            if (profilerConnected && (allocationsCheckBox.Checked || !callsCheckBox.Checked))
            {
                showHeapButton.Enabled = true;
            }
            else
            {
                showHeapButton.Enabled = false;
            }

            ToggleEvent(loggingActiveEvent, loggingActiveCompletedEvent);
            enableDisableAttachProcessButtonAndMenuItem();
        }

        private void callsCheckBox_CheckedChanged(object sender, System.EventArgs e)
        {
            if (profilerConnected && (allocationsCheckBox.Checked || !callsCheckBox.Checked))
            {
                showHeapButton.Enabled = true;
            }
            else
            {
                showHeapButton.Enabled = false;
            }

            ToggleEvent(callGraphActiveEvent, callGraphActiveCompletedEvent);
            enableDisableAttachProcessButtonAndMenuItem();
        }

        private void ResetStateAfterProfilingStopped()
        {
            bool connected = profilerConnected;
            ClearProfiledProcessInfo();

            logFileEndOffset = long.MaxValue;
            if (connected && log != null)
            {
                ReadLogResult readLogResult = GetLogResult();

                log.ReadFile(logFileStartOffset, logFileEndOffset, readLogResult);
                lastLogResult = readLogResult;
                EnableDisableViewMenuItems();
                ClearEvents();
                saveAsMenuItem.Enabled = true;
                viewSummaryMenuItem_Click(null, null);
            }

            EnableDisableLaunchControls(true);
            detachProcessButton.Enabled = showHeapButton.Enabled = killApplicationButton.Enabled = false;
            detachProcessMenuItem.Enabled = false;
            enableDisableAttachProcessButtonAndMenuItem();
        }

        private void checkProcessTimer_Tick(object sender, System.EventArgs e)
        {
            if (profiledProcess != null && ProfiledProcessHasExited())
            {
                ResetStateAfterProfilingStopped();
                return;
            }

            bool processRunning = profiledProcess != null && !ProfiledProcessHasExited();

            killApplicationButton.Enabled = processRunning;

            if (!allocationsCheckBox.Checked && callsCheckBox.Checked)
            {
                showHeapButton.Enabled = false;
            }
            else
            {
                showHeapButton.Enabled = processRunning && profilerConnected;
            }
        }
                
        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private void killApplicationButton_Click(object sender, System.EventArgs e)
        {
            if (profiledProcess != null)
            {
                if (killApplicationButton.Text == "Kill ASP.NET")
                {
                    StopIIS();
                    StartIIS();
                }
                else if (serviceName != null)
                {
                    StopService(serviceName, serviceStopCommand);
                }
                else
                {
                    profiledProcess.Kill();
                }

                ResetStateAfterProfilingStopped();
            }
        }

        private void setCommandLineMenuItem_Click(object sender, System.EventArgs e)
        {
            var setCommandLineForm = new SetParameterForm();
            setCommandLineForm.commandLineTextBox.Text = commandLine;
            setCommandLineForm.workingDirectoryTextBox.Text = workingDirectory;
            setCommandLineForm.logDirectoryTextBox.Text = GetLogDir();
            setCommandLineForm.gcOnLogFileCommentsCheckBox.Checked = gcOnLogFileComments;
            if (setCommandLineForm.ShowDialog() == DialogResult.OK)
            {
                commandLine = setCommandLineForm.commandLineTextBox.Text;
                workingDirectory = setCommandLineForm.workingDirectoryTextBox.Text;
                logDirectory = setCommandLineForm.logDirectoryTextBox.Text;
                gcOnLogFileComments = setCommandLineForm.gcOnLogFileCommentsCheckBox.Checked;
            }
        }

        private long logFileOffset()
        {
            Stream s = new FileStream(logFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            long offset = s.Length;
            s.Close();

            return offset;
        }

        private void showHeapButton_Click(object sender, System.EventArgs e)
        {
            forceGcCompletedEvent.Wait(1);
            forceGcCompletedEvent.Reset();
            long startOffset = logFileOffset();
            forceGcEvent.Set();
            const int maxIter = 10; // give up after ten minutes
            for (int iter = 0; iter < maxIter; iter++)
            {
                long lastOffset = logFileOffset();
                if (forceGcCompletedEvent.Wait(60 * 1000))
                {
                    forceGcCompletedEvent.Reset();
                    long saveLogFileStartOffset = logFileStartOffset;
                    logFileStartOffset = startOffset;
                    logFileEndOffset = logFileOffset();
                    ReadLogResult logResult = GetLogResult();
                    readLogFile(log, logResult, processFileName, Graph.GraphType.HeapGraph);
                    lastLogResult = logResult;
                    EnableDisableViewMenuItems();
                    logFileStartOffset = saveLogFileStartOffset;
                    break;
                }
                else
                {
                    // Hmm, the app didn't get back to us in 60 seconds
                    // If the log file is growing, assume the app is still dumping
                    // the heap, otherwise something is obviously wrong.
                    if (logFileOffset() == lastOffset)
                    {
                        MessageBox.Show("There was no response from the application");
                        break;
                    }
                }
            }
        }

        private void viewByAddressMenuItem_Click(object sender, System.EventArgs e)
        {
            var viewByAddressForm = new ViewByAddressForm();
            viewByAddressForm.Visible = true;
        }

        private void viewTimeLineMenuItem_Click(object sender, System.EventArgs e)
        {
            var timeLineViewForm = new TimeLineViewForm();
            timeLineViewForm.Visible = true;
        }

        private void viewHistogram_Click(object sender, System.EventArgs e)
        {
            var histogramViewForm = new HistogramViewForm();
            histogramViewForm.Visible = true;
        }

        private void saveAsMenuItem_Click(object sender, System.EventArgs e)
        {
            SaveFile();
        }

        private void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = !CheckProcessTerminate() || !CheckFileSave();
        }

        private void viewHistogramRelocatedMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Size for Relocated Objects";
                var histogramViewForm = new HistogramViewForm(lastLogResult.relocatedHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewHistogramFinalizerMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Size for Finalized Objects";
                var histogramViewForm = new HistogramViewForm(lastLogResult.finalizerHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewHistogramCriticalFinalizerMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Size for Critical Finalized Objects";
                var histogramViewForm = new HistogramViewForm(lastLogResult.criticalFinalizerHistogram, title);
                histogramViewForm.Show();
            }
        }

        private void viewAgeHistogram_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string title = "Histogram by Age for Live Objects";
                var ageHistogram = new AgeHistogram(lastLogResult.liveObjectTable, title);
                ageHistogram.Show();
            }
        }

        private void viewAllocationGraphmenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.AllocationGraph);
            }
        }

        private void viewAssemblyGraphmenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.AssemblyGraph);
            }
        }

        private void viewHeapGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.HeapGraph);
            }
        }

        private void viewCallGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.CallGraph);
            }
        }

        private void Form1_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.F5)
            {
                startApplicationButton_Click(null, null);
            }
        }

        private void profileServiceMenuItem_Click(object sender, System.EventArgs e)
        {
            var profileServiceForm = new ProfileServiceForm();
            if (profileServiceForm.ShowDialog() == DialogResult.OK)
            {
                serviceName = profileServiceForm.serviceNameTextBox.Text;
                serviceStartCommand = profileServiceForm.startCommandTextBox.Text;
                serviceStopCommand = profileServiceForm.stopCommandTextBox.Text;

                ProfileService();
            }
        }

        private void viewFunctionGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.FunctionGraph);
            }
        }

        private void viewModuleGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.ModuleGraph);
            }
        }

        private void viewClassGraphMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                ViewGraph(lastLogResult, processFileName, Graph.GraphType.ClassGraph);
            }
        }

        private void viewCommentsMenuItem_Click(object sender, System.EventArgs e)
        {
            if (log != null && log.commentEventList.count != 0)
            {
                var viewCommentsForm = new ViewCommentsForm(log);
                viewCommentsForm.Visible = true;
            }
        }

        private void viewCallTreeMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null && lastLogResult.hadCallInfo)
            {
                var callTreeForm = new CallTreeForm(log.fileName, lastLogResult);
            }
        }

        private void viewComparisonMenuItem_Click(object sender, System.EventArgs e)
        {
            currlogFileName = this.logFileName;

            var openFileDialog1 = new OpenFileDialog();
            openFileDialog1.FileName = "*.log";
            openFileDialog1.Filter = "Allocation Logs | *.log";
            if (openFileDialog1.ShowDialog() == DialogResult.OK && openFileDialog1.CheckFileExists)
            {
                prevlogFileName = openFileDialog1.FileName;
            }
            this.noUI = false;
            graphtype = Graph.GraphType.AllocationGraph;
            try
            {
                var _MgrForm = new ReportForm(this);
                _MgrForm.Visible = true;
            }
            catch
            {
                /* errors already told user, so continue. */
            }
        }

        private void viewSummaryMenuItem_Click(object sender, System.EventArgs e)
        {
            if (lastLogResult != null)
            {
                string scenario = log.fileName;
                if (processFileName != null)
                {
                    scenario = processFileName + " " + commandLine;
                }

                var summaryForm = new SummaryForm(log, lastLogResult, scenario);
                summaryForm.Show();
            }
        }

        private void targetCLRVersioncomboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            enableDisableAttachProcessButtonAndMenuItem();
        }

        private void startURLMenuItem_Click(object sender, EventArgs e)
        {
            startURLButton_Click(null, null);
        }

        [DllImport("ieframe.dll", SetLastError = true)]
        public static extern int IELaunchURL(
            [MarshalAs(UnmanagedType.LPWStr)] string url,
            ref PROCESS_INFORMATION pProcInfo,
            ref IELAUNCHURLINFO lpInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern void CloseHandle(
            IntPtr handle
        );

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private void startURLButton_Click(object sender, EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            if (!noUI)
            {
                //URL+Desktop Runtime is a legal choice,
                //But more likely a careless mistake.
                if (!targetv4CoreCLR())
                {
                    DialogResult dr = MessageBox.Show("You are about to profile the desktop CLR on a web page.  This is likely a mistake.\nIf you would like to profile Silverlight, please press cancel and select \"V4 Core CLR\".",
                                                      "CLRProfiler", MessageBoxButtons.OKCancel);
                    if (dr == DialogResult.Cancel)
                    {
                        return;
                    }
                }

                var openURLForm = new OpenURLForm();
                if (openURLForm.ShowDialog() == DialogResult.Cancel)
                {
                    return;
                }

                profilingURL = openURLForm.GetURL();
                if ((profilingURL == null) || (profilingURL == string.Empty))
                {
                    MessageBox.Show("URL is invalid.", "Failure");
                    return;
                }
            }

            try
            {
                if (targetv4CoreCLR())
                {
                    Environment.SetEnvironmentVariable("CoreCLR_Enable_Profiling", "0x1");
                    Environment.SetEnvironmentVariable("CORECLR_PROFILER", "{8C29BC4E-1F57-461a-9B51-1200C32E6F1F}");
                    Environment.SetEnvironmentVariable("CORECLR_PROFILER_PATH", getProfilerFullPath());
                }
                else
                {
                    Environment.SetEnvironmentVariable("Cor_Enable_Profiling", "0x1");
                    Environment.SetEnvironmentVariable("COR_PROFILER", "{8C29BC4E-1F57-461a-9B51-1200C32E6F1F}");
                    Environment.SetEnvironmentVariable("COR_PROFILER_PATH", getProfilerFullPath());
                }

                Environment.SetEnvironmentVariable("OMV_USAGE", CreateUsageString());
                Environment.SetEnvironmentVariable("OMV_SKIP", "0");
                Environment.SetEnvironmentVariable("OMV_PATH", GetLogDir());
                Environment.SetEnvironmentVariable("OMV_STACK", trackCallStacks ? "1" : "0");
                Environment.SetEnvironmentVariable("OMV_FORMAT", "v2");
                Environment.SetEnvironmentVariable("OMV_DynamicObjectTracking", "0x1");
                Environment.SetEnvironmentVariable("OMV_FORCE_GC_ON_COMMENT", gcOnLogFileComments ? "1" : "0");
                Environment.SetEnvironmentVariable("OMV_INITIAL_SETTING", CreateInitialString());
                Environment.SetEnvironmentVariable("OMV_TargetCLRVersion", targetv2DesktopCLR() ? "v2" : "v4");

                var pi = new PROCESS_INFORMATION();
                var IEli = new IELAUNCHURLINFO();
                IEli.cbSize = Marshal.SizeOf(typeof(IELAUNCHURLINFO));
                if (IELaunchURL(profilingURL, ref pi, ref IEli) == 0)
                {
                    CloseHandle(pi.hProcess);
                    CloseHandle(pi.hThread);

                    profiledProcess = Process.GetProcessById((int)pi.dwProcessId);

                    if (WaitForProcessToConnect(GetLogDir(), "Waiting for application to start common language runtime") <= 0)
                    {
                        ClearProfiledProcessInfo();
                    }
                }
            }
            catch
            {
                processFileName = "iexplore.exe";
                commandLine = profilingURL;
                startApplicationButton_Click(null, null);
            }
            finally
            {
                if (targetv4CoreCLR())
                {
                    Environment.SetEnvironmentVariable("CoreCLR_Enable_Profiling", "");
                    Environment.SetEnvironmentVariable("CORECLR_PROFILER", "");
                    Environment.SetEnvironmentVariable("CORECLR_PROFILER_PATH", "");
                }
                else
                {
                    Environment.SetEnvironmentVariable("Cor_Enable_Profiling", "");
                    Environment.SetEnvironmentVariable("COR_PROFILER", "");
                    Environment.SetEnvironmentVariable("COR_PROFILER_PATH", "");
                }

                Environment.SetEnvironmentVariable("OMV_USAGE", "");
                Environment.SetEnvironmentVariable("OMV_SKIP", "");
                Environment.SetEnvironmentVariable("OMV_PATH", "");
                Environment.SetEnvironmentVariable("OMV_STACK", "");
                Environment.SetEnvironmentVariable("OMV_FORMAT", "");
                Environment.SetEnvironmentVariable("OMV_DynamicObjectTracking", "");
                Environment.SetEnvironmentVariable("OMV_FORCE_GC_ON_COMMENT", "");
                Environment.SetEnvironmentVariable("OMV_INITIAL_SETTING", "");
                Environment.SetEnvironmentVariable("OMV_TargetCLRVersion", "");
            }
        }

        private void attachProcessMenuItem_Click(object sender, EventArgs e)
        {
            attachProcessButton_Click(null, null);
        }

        private void attachProcessButton_Click(object sender, EventArgs e)
        {
            if (!CheckProcessTerminate() || !CheckFileSave())
            {
                return;
            }

            var attachTargetPIDForm = new AttachTargetPIDForm();
            if (attachTargetPIDForm.ShowDialog() == DialogResult.Cancel)
            {
                return;
            }

            attachTargetPID = attachTargetPIDForm.GetPID();
            if (attachTargetPID == 0)
            {
                return;
            }

            if (attachProfiler(attachTargetPID, GetLogFullPath(attachTargetPID)))
            {
                allocationsCheckBox.Enabled = callsCheckBox.Enabled = false;
                detachProcessButton.Enabled = detachProcessMenuItem.Enabled = true;
            }
            else
            {
                logDirectory = null;
                ClearProfiledProcessInfo();
            }
        }

        private void detachProcessMenuItem_Click(object sender, EventArgs e)
        {
            detachProcessButton_Click(null, null);
        }


        private void detachProcessButton_Click(object sender, EventArgs e)
        {
            if (attachTargetPID == 0)
            {
                return;
            }

            SendDetachRequest();

            ResetStateAfterProfilingStopped();
        }

        private void SendDetachRequest()
        {
            detachEvent.Set();

            if (IsProfilingWindowsStoreApp())
            {
                // ResetStateAfterProfilingStopped() (called after this) will call DisableDebugging()
                // on the WindowsStoreApp, which allows the OS to go back to suspending the
                // WindowsStoreApp, and that would (annoyingly) prevent the profiler from getting
                // unloaded (until the user interacts with the app again).  So first,
                // give the WindowsStoreApp a little bit of time to unload the profiler while
                // it's still forced into the Running state.
                Thread.Sleep(6000);
            }
        }

        private void enableDisableAttachProcessButtonAndMenuItem()
        {
            if (profilingActiveCheckBox.Checked && targetv4DesktopCLR() && !allocationsCheckBox.Checked && !callsCheckBox.Checked && profiledProcess == null)
            {
                attachProcessButton.Enabled = attachProcessMenuItem.Enabled = true;
            }
            else
            {
                attachProcessButton.Enabled = attachProcessMenuItem.Enabled = false;
            }
        }

        private bool targetv4DesktopCLR()
        {
            return (noUI && targetCLRVersion == CLRSKU.V4DesktopCLR) || (!noUI && targetCLRVersioncomboBox.SelectedIndex == 0);
        }

        private bool targetv4CoreCLR()
        {
            return (noUI && targetCLRVersion == CLRSKU.V4CoreCLR) || (!noUI && targetCLRVersioncomboBox.SelectedIndex == 1);
        }

        private bool targetv2DesktopCLR()
        {
            return (noUI && targetCLRVersion == CLRSKU.V2DesktopCLR) || (!noUI && targetCLRVersioncomboBox.SelectedIndex == 2);
        }

        private string getProfilerFullPath()
        {
            return Path.GetDirectoryName(Application.ExecutablePath) + "\\ProfilerOBJ.dll";
        }

        private string getLogFileName(int pid)
        {
            return (nameToUse == null || nameToUse == "" ? string.Format("pipe_{0}.log", pid) : nameToUse);
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private int getPID(string[] arguments)
        {
            if (arguments.Length == 1)
            {
                Console.WriteLine("Please specify the process ID");
                return 0;
            }
            int pid = 0;
            try
            {
                pid = Int32.Parse(arguments[1]);
                Process.GetProcessById(pid);
            }
            catch (Exception e)
            {
                ShowErrorMessage( string.Format("The process ID ({0}) is not valid : {1}", arguments[1], e.Message) );
                pid = 0;
            }
            
            return pid;
        }

        private bool isProfilerLoaded(int pid)
        {
            NamedManualResetEvent forceGCEvent = CreateEvent("OMV_ForceGC", pid, false);
            bool result = forceGCEvent.IsValid();
            forceGCEvent.Dispose();
            return result;
        }

        private void ShowErrorMessage(string message)
        {
            if (!noUI)
            {
                MessageBox.Show(message, "CLRProfiler");
            }
            else
            {
                Console.WriteLine(message);
            }
        }

        [DllImport("profilerOBJ.dll", CharSet=CharSet.Unicode)]
        private static extern uint AttachProfiler(int pid, string targetVersion, string profilerPath, [In] ref ProfConfig profConfig, bool fConsoleMode);

        // Check if the specified pid is a WindowsStoreApp.  If so, initialize
        // a new windowsStoreAppProfileeInfo based on it.
        // If there's an error doing so, return false.  If we successfully
        // get the WindowsStoreApp info, or if pid isn't a WindowsStoreApp process at all,
        // return true.
        private bool InitWindowsStoreAppProfileeInfoIfNecessary(int pid)
        {
            WindowsStoreAppHelperWrapper.Init();
            if (WindowsStoreAppHelperWrapper.IsWindowsStoreAppSupported())
            {
                string acSid;
                string acFolderPath;
                string windowsStoreAppPackageFullName = null;
                WindowsStoreAppHelperWrapper.GetWindowsStoreAppInfoFromProcessId(pid, out acSid, out acFolderPath, out windowsStoreAppPackageFullName);
                if (acSid != null)
                {
                    // Ensure WindowsStoreApp packages will have access to CLRProfiler binaries
                    if (!WindowsStoreAppHelperWrapper.IsWindowsStoreAppAccessEnabledForProfiler(Path.GetDirectoryName(Application.ExecutablePath)))
                    {
                        return false;
                    }

                    windowsStoreAppProfileeInfo = new WindowsStoreAppProfileeInfo(windowsStoreAppPackageFullName, acSid);
                    InitWindowsStoreAppLogDirectory(acFolderPath);

                    // Before we send the attach signal to the profilee, we
                    // want to enable "debugging" mode, to ensure it's awake
                    // and won't get suspended
                    WindowsStoreAppHelperWrapper.EnableDebuggingForPackage(windowsStoreAppPackageFullName);
                }
            }
            else
            {
                startWindowsStoreAppButton.Enabled = false;
            }

            return true;
        }

        [SuppressMessage("Microsoft.Security", "CA2122:DoNotIndirectlyExposeMethodsWithLinkDemands", Justification = "CLRProfiler.exe is a stand-alone tool, not a library.")]
        private bool attachProfiler(int pid, string fileName)
        {
            if (isProfilerLoaded(pid))
            {
                ShowErrorMessage("CLRProfiler is already loaded in the target process.");
                return false;
            }

            if (!InitWindowsStoreAppProfileeInfoIfNecessary(pid))
            {
                return false;
            }


            // For WindowsStoreApps, there's no choice of where the log file can be, so
            // override any request
            if (IsProfilingWindowsStoreApp())
            {
                fileName = GetLogFullPath(pid);
            }

            var config = new ProfConfig();
            config.usage = OmvUsage.OmvUsageNone;
            config.bOldFormat = 0;
            config.szFileName = fileName;
            config.bDynamic = 0;
            config.bStack = 0;
            config.dwSkipObjects = 0;
            config.szClassToMonitor = String.Empty;
            config.dwInitialSetting = 0;
            config.dwDefaultTimeoutMs = maxWaitingTimeInMiliseconds;
            config.bWindowsStoreApp = IsProfilingWindowsStoreApp();

            uint result = AttachProfiler(pid, "v4.", getProfilerFullPath(), ref config, noUI);

            profiledProcess = Process.GetProcessById(pid);
            bool ret;
            if (IsProfilingWindowsStoreApp())
            {
                ret = (WaitForWindowsStoreAppProcessToConnect((uint)pid, "Waiting for Windows Store application to start common language runtime") > 0);
            }
            else
            {
                ret = (WaitForProcessToConnect(GetLogDir(), "Waiting for application to load the CLRProfiler", true, result) > 0);
            }

            if (ret)
            {
                return true;
            }
            else
            {
                ClearProfiledProcessInfo();
                return false;
            }
        }

        private void menuStartWindowsStoreApp_Click(object sender, EventArgs e)
        {
            startWindowsStoreAppButton_Click(sender, e);
        }
    }
}
