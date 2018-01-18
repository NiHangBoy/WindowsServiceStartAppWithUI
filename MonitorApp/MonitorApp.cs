using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Security;
using System.Threading;
using static MonitorApp.WindowsApi;
using System.Runtime.InteropServices;

namespace MonitorApp
{
    public partial class MonitorApp : ServiceBase
    {
        private bool _running;
        private List<App> _apps;
        private EventLog _log;

        public MonitorApp()
        {
            InitializeComponent();

            string eventSourceName = $"{ServiceName}Source";
            string logName = $"{ServiceName}Log";
            _log = new EventLog();
            if (!EventLog.SourceExists(eventSourceName))
            {
                EventLog.CreateEventSource(eventSourceName, logName);
            }
            _log.Source = eventSourceName;
            _log.Log = logName;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                _running = true;
                Task.Factory.StartNew(new Action(Monitor), TaskCreationOptions.LongRunning);
            }
            catch (Exception ex)
            {
                WriteLog($"OnStart {EventLogEntryType.Error}, {ex.Message}");
            }
        }

        protected override void OnStop()
        {
            _running = false;
        }

        void Monitor()
        {
            try
            {
                _apps = GetConfig();
                while (_running)
                {
                    Process[] allProcess = Process.GetProcesses();
                    // Write all processes to text
                    string path = AppDomain.CurrentDomain.BaseDirectory + @"SystemProcess.txt";
                    using (StreamWriter wr = new StreamWriter(path, false))
                    {
                        foreach (var p in allProcess)
                        {
                            var item = p.Id + "  " + p.ProcessName;
                            wr.WriteLine(item);
                        }
                    }

                    if (_apps != null)
                    {
                        foreach (var app in _apps)
                        {
                            var ps = allProcess.Any(p => { return p.ProcessName.Equals(app.Process); });
                            if (!ps)
                            {
                                IntPtr dupeTokenHandle = IntPtr.Zero;
                                IntPtr hProcess = Process.GetCurrentProcess().Handle;
                                IntPtr hTargetToken = IntPtr.Zero;
                                var sa = new SECURITY_ATTRIBUTES
                                {
                                    bInheritHandle = false
                                };
                                sa.nLength = Marshal.SizeOf(sa);
                                sa.lpSecurityDescriptor = 0;
                                var processInfo = new PROCESS_INFORMATION();
                                var startInfo = new STARTUPINFO();
                                startInfo.cb = Marshal.SizeOf(startInfo);
                                startInfo.lpDesktop = "";

                                bool status = DuplicateTokenEx(
                                    dupeTokenHandle, 
                                    TOKEN_ALL_ACCESS, 
                                    ref sa, 
                                    SecurityImpersonation, 
                                    TokenPrimary, 
                                    ref hTargetToken);

                                if (!status)
                                {
                                    int err1 = Marshal.GetLastWin32Error();
                                    WriteLog("TokenHandle:" + err1);
                                }

                                WTSQueryUserToken(WTSGetActiveConsoleSessionId(), ref hTargetToken);

                                bool ret = CreateProcessAsUser( 
                                    hTargetToken,
                                    app.Path,
                                    null,
                                    IntPtr.Zero, 
                                    IntPtr.Zero,
                                    false,
                                    CREATE_NEW_CONSOLE,
                                    IntPtr.Zero,
                                    null,
                                    ref startInfo, 
                                    out processInfo);

                                if (!ret) throw new Exception("CreateProcessAsUser failed");
                            }
                        }
                    }
                    Thread.Sleep(2000);
                }
            }
            catch (Exception ex)
            {
                WriteLog($"Monitor {ex.Message}", EventLogEntryType.Error);
                Stop();
                throw new Exception("Monitor Exception ", ex);
            }
        }

        // Load Config.xml and get configrations
        private List<App> GetConfig()
        {
            string configPath = AppDomain.CurrentDomain.BaseDirectory + @"Config.xml";
            if (!File.Exists(configPath)) return null;
            var doc = XDocument.Load(configPath);
            var apps = from app in doc.Descendants("App")
                       select new App
                       {
                           Path = app.Attribute("Path").Value,
                           Process = app.Attribute("Process").Value
                       };
            return apps.ToList();
        }

        public void Test(string[] args)
        {
            OnStart(args);
            Console.ReadLine();
        }

        private void WriteLog(string s, EventLogEntryType logtype = EventLogEntryType.Information)
        {
            Trace.WriteLine($"{GetType().Name}.{s}");
            _log.WriteEntry(s, logtype);
        }

        private int GetProcId(string commandArgs)
        {
            Process[] p = null;
            var processName = Path.GetFileNameWithoutExtension(commandArgs);
            for (int i = 1; i <= 5; i++)
            {
                WriteLog($"OnStart GetProcessByName i={i} processname={processName}");
                p = Process.GetProcessesByName(processName);
                if (p != null && p.Length > 0) break;
                Thread.Sleep(1000);
            }
            WriteLog($"OnStart p.Length={p.Length}");
            if (p.Length == 0)
            {
                throw new Exception($"Command {commandArgs} has not started. Check the Windows Application Log");
            }
            else
            {
                if (p.Length > 1)
                {
                    throw new Exception($"More than one process found named {commandArgs}");
                }
            }
            return p[0].Id;
        }
    }

    internal class App
    {
        public string Path { get; set; }
        public string Process { get; set; }
    }
}
