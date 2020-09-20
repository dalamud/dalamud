using System;
using System.Buffers;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using Dalamud.Bootstrap.FFI;

namespace Dalamud.Bootstrap
{
    /// <summary>
    /// Thin wrapper over child process handles to implement Dispose()
    /// </summary>
    /// <remarks>
    /// Currently process is assumed to be 64 bit.
    /// </remarks>
    sealed class ChildProcess : IDisposable
    {
        [DllImport("ntdll.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static unsafe extern NTSTATUS NtQueryInformationProcess(
            nint hProcess,
            uint processInformationClass,
            void* ProcessInformation,
            uint processInformationLength,
            out uint returnLength
        );

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(nint handle);

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static unsafe extern bool WriteProcessMemory(
            nint hProcess,
            nint lpBaseAddress,
            void* lpBuffer,
            nint nSize,
            out nint lpNumberOfBytesWritten
        );

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static unsafe extern bool ReadProcessMemory(
            nint hProcess,
            nint lpBaseAddress,
            void* lpBuffer,
            nint nSize,
            out nint lpNumberOfBytesRead
        );

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool TerminateProcess(nint hProcess, uint uExitCode);

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern uint GetProcessId(nint hProcess);

        [DllImport("kernel32.dll", ExactSpelling = true, CallingConvention = CallingConvention.Winapi, SetLastError = true)]
        private static extern nint VirtualAllocEx(nint hProcess, nint lpAddress, nint dwSize, uint flAllocationType, uint flProtect);

        public ChildProcess(nint mainProcessHandle, nint mainThreadHandle)
        {
            Handle = mainProcessHandle;
            MainThread = mainThreadHandle;
        }

        ~ChildProcess()
        {
            DisposeImpl();
        }

        public nint Handle { get; private set; }
        public nint MainThread { get; private set; }

        public void Dispose()
        {
            DisposeImpl();
            GC.SuppressFinalize(this);
        }

        private void DisposeImpl()
        {
            CloseHandle(Handle);
            CloseHandle(MainThread);

            Handle = 0;
            MainThread = 0;
        }

        public uint Id => GetProcessId(Handle);

        public nint VirtualAlloc(nint size, uint flAllocationType, uint mprotect)
        {
            var ptr = VirtualAllocEx(Handle, default, size, flAllocationType, mprotect);
            if (ptr == 0)
            {
                throw new Win32Exception();
            }

            return ptr;
        }

        public void ReadMemory(nint address, Span<byte> buffer)
        {
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    nint offset = 0;

                    while (offset < buffer.Length)
                    {
                        if (!ReadProcessMemory(Handle, address + offset, pBuffer + offset, buffer.Length - offset, out var bytesRead))
                        {
                            throw new Win32Exception();
                        }

                        if (bytesRead == 0)
                        {
                            // can't read this area
                            throw new Win32Exception();
                        }

                        offset = checked(offset + bytesRead);
                    }
                }
            }
        }

        public void ReadMemory<T>(nint address, ref T value) where T : unmanaged
        {
            var span = MemoryMarshal.CreateSpan(ref value, 1);
            var bytes = MemoryMarshal.AsBytes(span);

            ReadMemory(address, bytes);
        }

        public void WriteMemory(nint address, ReadOnlySpan<byte> buffer)
        {
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    nint offset = 0;

                    while (offset < buffer.Length)
                    {
                        if (!WriteProcessMemory(Handle, address + offset, pBuffer + offset, buffer.Length - offset, out var bytesWritten))
                        {
                            throw new Win32Exception();
                        }

                        if (bytesWritten == 0)
                        {
                            // can't read this area
                            throw new Win32Exception();
                        }

                        offset = checked(offset + bytesWritten);
                    }
                }
            }
        }

        public void WriteMemory<T>(nint address, ref T value) where T : unmanaged
        {
            var span = MemoryMarshal.CreateSpan(ref value, 1);
            var bytes = MemoryMarshal.AsBytes(span);

            WriteMemory(address, bytes);
        }

        public void WriteMemory<T>(nint address, T value) where T: unmanaged
        {
            var span = MemoryMarshal.CreateSpan(ref value, 1);
            var bytes = MemoryMarshal.AsBytes(span);

            WriteMemory(address, bytes);
        }

        private string ReadUnicodeStringFromMemory(nint address)
        {
            UNICODE_STRING unicodeString = default;
            ReadMemory(address, ref unicodeString);

            // https://docs.microsoft.com/en-us/windows/win32/api/ntdef/ns-ntdef-_unicode_string
            // Length is "in bytes"
            using var buffer = MemoryPool<byte>.Shared.Rent(unicodeString.Length);
            var span = buffer.Memory.Span[..unicodeString.Length];
            ReadMemory(unicodeString.Buffer, span);

            return Encoding.Unicode.GetString(span);
        }

        public void Terminate(uint exitCode)
        {
            if (!TerminateProcess(Handle, exitCode))
            {
                throw new Win32Exception();
            }
        }

        public string ImagePath
        {
            get
            {
                nint procParamAddr = default;
                ReadMemory(GetPebAddress() + 0x20, ref procParamAddr);
                
                return ReadUnicodeStringFromMemory(procParamAddr + 0x60);
            }
        }

        public nint GetEntryPointAddress()
        {
            IMAGE_DOS_HEADER dosHeader = default;
            IMAGE_NT_HEADERS64 ntHeader = default;
            
            var baseAddress = GetBaseAddress();

            ReadMemory(baseAddress, ref dosHeader);
            ReadMemory(baseAddress + (int) dosHeader.e_lfanew, ref ntHeader);

            return baseAddress + (int) ntHeader.OptionalHeader.AddressOfEntryPoint;
        }

        public nint GetPebAddress()
        {
            PROCESS_BASIC_INFORMATION info = default;
            ReadProcessBasicInfo(ref info);

            return info.PebBaseAddress;
        }

        public nint GetBaseAddress()
        {
            nint baseAddress = default;
            ReadMemory(GetPebAddress() + 0x10, ref baseAddress); // assumed to be 64 bit

            return baseAddress;
        }

        private void ReadProcessBasicInfo(ref PROCESS_BASIC_INFORMATION info)
        {
            unsafe
            {
                fixed (PROCESS_BASIC_INFORMATION* pInfo = &info)
                {
                    var status = NtQueryInformationProcess(Handle, 0, pInfo, (uint) sizeof(PROCESS_BASIC_INFORMATION), out var _);
                    if (!status.Success)
                    {
                        new InvalidOperationException($"NtQueryInformationProcess failed with status code {status}");
                    }
                }
            }
        }
    }
}
