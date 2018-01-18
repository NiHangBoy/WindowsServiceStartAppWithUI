using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MonitorApp
{
    public static class WindowsApi
    {
        [DllImport("kernel32.dll", EntryPoint = "WTSGetActiveConsoleSessionId", SetLastError = true)]
        public static extern uint WTSGetActiveConsoleSessionId();

        [DllImport("wtsapi32.dll", EntryPoint = "WTSQueryUserToken", SetLastError = true)]
        public static extern bool WTSQueryUserToken(uint sessionId, ref IntPtr phToken);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        internal static extern bool DuplicateTokenEx(IntPtr tokenHandle, int
              dwDesiredAccess,
              ref SECURITY_ATTRIBUTES lpTokenAttributes, 
              int SECURITY_IMPERSONATION_LEVEL,
              int TOKEN_TYPE, ref IntPtr dupeTokenHandle);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool DuplicateHandle(
            IntPtr hSourceProcessHandle,
            IntPtr hSourceHandle,
            IntPtr hTargetProcessHandle,
            out IntPtr lpTargetHandle,
            uint dwDesiredAccess, 
            bool bInheritHandle, 
            uint dwOptions);

        [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool CreateProcessAsUser(
               IntPtr hToken,
               String lpApplicationName,
               String lpCommandLine,
               IntPtr lpProcessAttributes,
               IntPtr lpThreadAttributes,
               Boolean bInheritHandles,
               UInt32 dwCreationFlags,
               IntPtr lpEnvironment,
               String lpCurrentDirectory,
               ref STARTUPINFO lpStartupInfo,
               out PROCESS_INFORMATION lpProcessInformation);

        [StructLayout(LayoutKind.Sequential)]
        public struct SECURITY_ATTRIBUTES
        {
            public int nLength;
            public int lpSecurityDescriptor;
            public bool bInheritHandle;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct STARTUPINFO
        {
            public int cb;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpReserved;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpDesktop;
            [MarshalAs(UnmanagedType.LPTStr)]
            public string lpTitle;
            public int dwX;
            public int dwY;
            public int dwXSize;
            public int dwYSize;
            public int dwXCountChars;
            public int dwYCountChars;
            public int dwFillAttribute;
            public int dwFlags;
            public short wShowWindow;
            public short cbReserved2;
            public IntPtr lpReserved2;
            public IntPtr hStdInput;
            public IntPtr hStdOutput;
            public IntPtr hStdError;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PROCESS_INFORMATION
        {
            public IntPtr hProcess;
            public IntPtr hThread;
            public int dwProcessId;
            public int dwThreadId;
        }

        // SECURITY_IMPERSONATION_LEVEL
         public const  int SecurityAnonymous = 0;
         public const int SecurityIdentification = 1;
         public const int SecurityImpersonation = 2;
         public const int SecurityDelegation = 3;

        // TOKEN_TYPE
         public const int TokenPrimary = 1;
         public const int TokenImpersonation = 2;

        //dwLogonFlags Specifies the logon option
         public const int LOGON_WITH_PROFILE = 1;
         public const int LOGON_NETCREDENTIALS_ONLY = 2;

        // Access Token  public constants
         public const int TOKEN_ALL_ACCESS = 0x10000000;

        //dwCreationFlags - Specifies how the process is created
         public const int DETACHED_PROCESS = 0x00000008;
         public const int CREATE_NO_WINDOW = 0x08000000;
         public const int CREATE_SUSPENDED = 0x00000004;
         public const int CREATE_NEW_CONSOLE = 0x00000010;
         public const int CREATE_NEW_PROCESS_GROUP = 0x00000200;
         public const int CREATE_SEPARATE_WOW_VDM = 0x00000800;
         public const int CREATE_UNICODE_ENVIRONMENT = 0x00000400;
         public const int CREATE_DEFAULT_ERROR_MODE = 0x04000000;

        //dwCreationFlags parameter controls the new process's priority class
         public const int NORMAL_PRIORITY_CLASS = 0x00000020;
         public const int IDLE_PRIORITY_CLASS = 0x00000040;
         public const int HIGH_PRIORITY_CLASS = 0x00000080;
         public const int REALTIME_PRIORITY_CLASS = 0x00000100;
         public const int BELOW_NORMAL_PRIORITY_CLASS = 0x00004000;
         public const int ABOVE_NORMAL_PRIORITY_CLASS = 0x00008000;
        //dwFlags
        // This is a bit field that determines whether certain STARTUPINFO
        // members are used when the process creates a window.
        // Any combination of the following values can be specified:
         public const int STARTF_USESHOWWINDOW = 0x0000000;
         public const int STARTF_USESIZE = 0x00000002;
         public const int STARTF_USEPOSITION = 0x00000004;
         public const int STARTF_USECOUNTCHARS = 0x00000008;
         public const int STARTF_USEFILLATTRIBUTE = 0x00000010;
         public const int STARTF_FORCEONFEEDBACK = 0x00000040;
         public const int STARTF_FORCEOFFFEEDBACK = 0x00000080;
         public const int STARTF_USESTDHANDLES = 0x00000100;
         public const int STARTF_USEHOTKEY = 0x00000200;
    }
}
