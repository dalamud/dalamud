using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using Dalamud.Bootstrap.FFI;
using Dalamud.SqexArg;
using Reloaded.Injector;

namespace Dalamud.Bootstrap
{
    public class Bootstrapper
    {
        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetThreadContext(nint hThread, ref CONTEXT context);

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern uint ResumeThread(nint hThread);

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern uint SuspendThread(nint hThread);

        /// <summary>
        /// A canonicalized path to the program.
        /// </summary>
        private string m_program;

        private BootstrapperContext m_context;

        /// <summary>
        /// Contains launch configs that can be passed to the game.
        /// </summary>
        private SqexArgumentBuilder m_arguments;

        /// <summary>
        /// Contains properties related to win32 process spawning.
        /// </summary>
        private ProcessBuilder m_builder;

        /// <summary>
        /// Creates a new bootstrapper.
        /// </summary>
        /// <param name="program">A path to the program.</param>
        public Bootstrapper(string program, BootstrapperContext context) =>
            (m_context, m_program,                 m_arguments, m_builder) =
            (context,   Path.GetFullPath(program), new(),       new(program)
        );

        /// <summary>
        /// Adds an argument string that is directly passed to the game. (e.g. DEV.TestSID)
        /// </summary>
        /// <remarks>
        /// Please keep in mind that key is case sensitive. Which means DEV.TestSid and DEV.TestSID is not the same argument.
        /// 
        /// Also `T` (requires for launching) will be automatically added if not present.
        /// </remarks>
        public Bootstrapper Argument(string key, string value)
        {
            m_arguments.Add(key, value);

            return this;
        }

        /// <summary>
        /// Adds environment variable.
        /// </summary>
        public Bootstrapper Environment(string key, string value)
        {
            m_builder.Environment(key, value);

            return this;
        }

        /// <summary>
        /// Builds arguments pushed from Argument() to sqex argument (//**sqex0003**//) string from current time on the local machine.
        /// </summary>
        private string BuildSqexArgument()
        {
            var sqexArgKey = Key.FromCurrentTime();
            var sqexArg = m_arguments.Build(sqexArgKey);

            return sqexArg;
        }

        public void Spawn()
        {
            var bootContextStr = JsonSerializer.Serialize(m_context);
            
            var status = Spawn(bootContextStr);
            if (status != 0) {
                throw new Exception($"Failed to launch dalamud (status code: {status:X})");
            }
        }

        /// <summary>
        /// Creates a new game process and launch Dalamud inside it.
        /// </summary>
        /// <param name="bootContextStr">
        /// A string parameter to pass to dalamud_boot. This will be allocated via VirtualAlloc on the spawned process and written to that address
        /// with C-style (null terminated) string with UTF-8 encoding.
        /// </param>
        /// <returns>
        /// A status code returned from dalamud_boot::dalamud_init function. If there was an error, it will be non-zero value.
        /// </returns>
        private int Spawn(string bootContextStr)
        {
            // Bootstrapping process tl;dr;
            // 1. spawn a process with CREATE_SUSPENDED
            // 2. Note that when the game process spawns with CREATE_SUSPENDED,
            //    user-land process initialization is still unfinished at this point.
            //    You can't make use of many useful memory data including PEB and kernel32.dll.dll
            //    but only ntdll.dll and ffxiv_dx11.exe (main module) 
            // 3. So we'll let main thread running until just before the very entry-point of ffxiv_dx11.exe is executed.
            //    At which point we can assume process is fully initialized and therefore can safely inject our code.
            // 4. Inject dll using Reloaded.Hooking and pass hande out rest of the Dalamud initialization process to that code.

            // since the game inherits working directory from its launcher this is "technically" wrong cwd to point to
            // but we'll ignore it and use "what should have been" value which is same directory as ffxiv_dx11.exe
            var cwd = Path.GetDirectoryName(m_program);
            var argument = BuildSqexArgument();

            // Spawn a new process with its main thread suspended
            using var child = m_builder
                .WorkingDirectory(cwd)
                .Argument(argument)
                .Flags(0x0004) // CREATE_SUSPENDED
                .Spawn();

            // from here on we should just kill the spawned process if there's any error during dalamud initialization;
            // we don't any zombie process linger in the system.
            try
            {
                AwaitProcessInitalization(child);

                var childProcess = Process.GetProcessById((int) child.Id);
                using var injector = new Injector(childProcess);

                // Inject dll
                {
                    // load dependencies required by dalamud_boot
                    var nethostPath = Path.Combine(m_context.DalamudRoot, "nethost.dll");
                    var nethostModuleAddr = injector.Inject(nethostPath);
                    if (nethostModuleAddr == 0)
                    {
                        // maybe nethost.dll was not found in dalamud_root?
                        throw new Exception("Failed to inject nethost.dll");
                    }

                    // load dalamud_boot
                    var dllPath = Path.Combine(m_context.DalamudRoot, "dalamud_boot.dll");
                    var moduleAddr = injector.Inject(dllPath);
                    if (moduleAddr == 0)
                    {
                        // this can happen if nethost.dll is not loaded inside of the process.
                        throw new Exception("Failed to inject dalamud_boot.dll"); 
                    }
                }

                // Write a param then call dalamud_init
                {
                    var bootContextData = Encoding.UTF8.GetBytes(bootContextStr);
                    var allocMem = child.VirtualAlloc(
                        bootContextData.Length + 1, // string + 1 byte null terminator
                        0x1000 /* MEM_COMMIT */,
                        0x04 /* PAGE_READWRITE */
                    );
                    child.WriteMemory(allocMem, bootContextData);

                    return injector.CallFunction("dalamud_boot.dll", "dalamud_init", (long) allocMem);
                }
            }
            catch
            {
                child.Terminate(0xDAD_E_0001); // dalamud error 0001
                throw;
            }
        }

        /// <summary>
        /// Wait for process initialization to finish.
        /// </summary>
        /// <remarks>
        /// Current implementation expects the child process to be spawned with CREATE_SUSPENDED flag and main thread is not resumed.
        /// </remarks>
        /// <param name="handle"></param>
        private void AwaitProcessInitalization(ChildProcess childProcess)
        {
            static void AwaitProcessInitalizationImpl(ChildProcess childProcess, nint entryPointAddr)
            {
                // poll thread context
                CONTEXT threadContext = default;
                while (true)
                {
                    // it's assumed that there's no need to pause a thread
                    threadContext.ContextFlags = 0x0010_0000 | 0x0000_0001; // CONTEXT_AMD64 | CONTEXT_CONTROL 
                    if (!GetThreadContext(childProcess.MainThread, ref threadContext))
                    {
                        throw new Win32Exception();
                    }

                    if (threadContext.Rip == entryPointAddr)
                    {
                        break;
                    }

                    Thread.Sleep(1);
                }
            }

            // Get an address of main module (.exe) entry-point
            var entryPointAddr = childProcess.GetEntryPointAddress();

            // A buffer to store original instructions
            Span<byte> originalInst = stackalloc byte[2];
            Span<byte> jumpInst = stackalloc byte[] {
                0xEB, 0xFE // jmp -02; which means it will execute jmp -02 over and over 
            };

            unsafe
            {
                fixed (byte* pOriginalInst = originalInst)
                fixed (byte* pJumpIst = jumpInst)
                {
                    // store original instructions
                    childProcess.ReadMemory(entryPointAddr, originalInst);

                    // overwrite instructions
                    childProcess.WriteMemory(entryPointAddr, jumpInst);

                    // Resume main thread and let the child process handle its process initialization, then suspend again.
                    ResumeThread(childProcess.MainThread);
                    AwaitProcessInitalizationImpl(childProcess, entryPointAddr);
                    SuspendThread(childProcess.MainThread);

                    // restore original instructions
                    childProcess.WriteMemory(entryPointAddr, originalInst);
                }
            }
        }
    }
}
