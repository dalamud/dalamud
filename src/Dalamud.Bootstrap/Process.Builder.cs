using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Bootstrap.FFI;

namespace Dalamud.Bootstrap
{
    class ProcessBuilder
    {
        [DllImport("Kernel32", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateProcessW(
           [MarshalAs(UnmanagedType.LPWStr)] string lpApplicationName,
           [MarshalAs(UnmanagedType.LPWStr)] StringBuilder lpCommandLine,
           nint lpProcessAttributes,
           nint lpThreadAttributes,
           [MarshalAs(UnmanagedType.Bool)] bool bInheritHandles,
           uint dwCreationFlags,
           [MarshalAs(UnmanagedType.LPWStr)] string? lpEnvironment,
           [MarshalAs(UnmanagedType.LPWStr)] string? lpCurrentDirectory,
           ref STARTUP_INFO lpStartupInfo,
           ref PROCESS_INFORMATION lpProcessInformation
       );

        [DllImport("Kernel32", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(nint handle);
        
        private string m_program;
        private StringBuilder m_commandLine;
        private EnvironmentBlock m_environments;
        private string? m_cwd;
        private uint m_flags;

        public ProcessBuilder(string program)
        {
            m_program = program;
            m_commandLine = new();
            m_environments = EnvironmentBlock.Capture();
            m_cwd = null;
            m_flags = 0;

            // argc[0] is always the path to the program by C convention
            new CommandLineWriter(m_commandLine)
                .WriteArgument(program);
        }

        /// <summary>
        /// Set process creation flags. This will always be or'd with CREATE_UNICODE_ENVIRONMENT flag.
        /// </summary>
        public ProcessBuilder Flags(uint flag)
        {
            m_flags = flag;

            return this;
        }

        public ProcessBuilder Argument(string arg)
        {
            var writer = new CommandLineWriter(m_commandLine);
            writer.WriteDelimiter();
            writer.WriteArgument(arg);

            return this;
        }

        public ProcessBuilder Environment(string key, string value)
        {
            m_environments.Add(key, value);

            return this;
        }

        public ProcessBuilder WorkingDirectory(string? cwd)
        {
            m_cwd = cwd;

            return this;
        }

        /// <summary>
        /// Spawn a new process.
        /// </summary>
        public ChildProcess Spawn()
        {
            unsafe
            {
                var startInfo = new STARTUP_INFO
                {
                    cb = (uint) sizeof(STARTUP_INFO),
                };
                var procInfo = new PROCESS_INFORMATION();

                var env = m_environments.ToString();
                
                var success = CreateProcessW(
                    m_program,
                    m_commandLine,
                    default,
                    default,
                    false,
                    m_flags | 0x400, // CREATE_UNICODE_ENVIRONMENT
                    env,
                    m_cwd,
                    ref startInfo,
                    ref procInfo
                );

                if (!success)
                {
                    throw new Win32Exception();
                }

                return new ChildProcess(procInfo.hProcess, procInfo.hThread);
            }
        }
    }
}
