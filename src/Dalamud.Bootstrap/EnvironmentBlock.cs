using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace Dalamud.Bootstrap
{
    class EnvironmentBlock
    {
        [DllImport("Advapi32", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenThreadToken(
            nint ThreadHandle,
            uint DesiredAccess,
            [MarshalAs(UnmanagedType.Bool)] bool OpenAsSelf,
            out nint TokenHandle
        );

        [DllImport("Userenv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CreateEnvironmentBlock(
            out nint lpEnvironment,
            nint hToken,
            [MarshalAs(UnmanagedType.Bool)] bool bInherit
        );

        [DllImport("Userenv", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool DestroyEnvironmentBlock(nint lpEnvironment);

        // https://referencesource.microsoft.com/#System/compmod/microsoft/win32/UnsafeNativeMethods.cs,195
        private static nint CurrentProcessToken => -4;

        private struct Entry
        {
            public string Key;
            public string Value;
        }

        private readonly Dictionary<string, Entry> m_environments = new();

        public void Add(string key, string value)
        {
            // From MSDN:
            // Because the equal sign is used as a separator, it must not be used in the name of an environment variable.
            if (key.Contains('='))
            {
                // TODO: tbh, nul characters are also not allowed but we do not check for that atm 
                throw new ArgumentException("An equal sign (=) is not allowed in key string.", nameof(key));
            }

            AddUnchecked(key, value);
        }

        private void AddUnchecked(string key, string value)
        {
            // on Windows key is case-insensitive yet still case preserving
            var upperKey = key.ToUpperInvariant();
            Entry entry = new()
            {
                Key = key,
                Value = value,
            };

            m_environments.Add(upperKey, entry);
        }
        
        public override string ToString()
        {   
            StringBuilder buffer = new();
            EnvironmentBlockWriter writer = new(buffer);

            foreach (var kv in m_environments)
            {
                var entry = kv.Value;
                writer.WriteEntry(entry.Key, entry.Value);
            }

            writer.WriteTerminator();
            
            return buffer.ToString();
        }

        /// <summary>
        /// Captures environment variables from the current process.
        /// </summary>
        /// <returns></returns>
        public static EnvironmentBlock Capture()
        {
            if (!CreateEnvironmentBlock(out var envBlock, CurrentProcessToken, false))
            {
                throw new Win32Exception();
            }

            try
            {
                var env = new EnvironmentBlock();
                env.CopyFromRawPointer(envBlock);

                return env;
            }
            finally
            {
                DestroyEnvironmentBlock(envBlock);
            }
        }

        /// <summary>
        /// Copies environment variables from the pointer to the environment blocked created by CreateEnvironmentBlock Win32 API.
        /// </summary>
        /// <param name="envBlock">A pointer to the environment block. This pointer be valid while this function is running.</param>
        private void CopyFromRawPointer(nint envBlock)
        {
            unsafe
            {
                var pEnvBlock = (char*) envBlock;
                var length = 0; // it is very UB to overflow (which can happen if entry is around >2GB

                while (true)
                {
                    var chr = pEnvBlock[checked(length++)];
                    
                    // nul terminator
                    if (chr == '\0')
                    {
                        // if string is empty (i.e. only the nul char) that means there are no more elements in the block
                        if (length <= 1)
                        {
                            break;
                        }

                        // NOTE: length is number of chars not bytes and INCLUDES nul terminator
                        var entry = new string(pEnvBlock, 0, length - 1); // don't include nul char
                        
                        // we assume here that the string generated from CreateEnvironmentBlock is always correctly delimited with eq(=) character
                        var kv = entry.Split('=', 2);
                        AddUnchecked(kv[0], kv[1]);

                        pEnvBlock += length; // set pEnvBlock to where the next entry is
                        length = 0;          // and reset length
                    }          
                }
            }
        }
    }
}
