using System;
using System.Buffers;
using System.Buffers.Text;

namespace Dalamud.SqexArg
{
    public struct Key
    {
        private static readonly char[] Checksums = {
            'f', 'X', '1', 'p', 'G', 't', 'd', 'S',
            '5', 'C', 'A', 'P', '4', '_', 'V', 'L',
        };

        public uint Tick { get; set; }

        public char Checksum
        {
            get
            {
                var index = (Tick & 0x000F_0000) >> 16; // basically 4th nibble value can be mapped to checksum 1:1
                return Checksums[index];
            }
        }

        /// Creates a new key based on current time on the local machine.
        public static Key FromCurrentTime() => new Key
        {
            Tick = (uint) Environment.TickCount
        };

        public bool TryFormat(Span<byte> buffer)
        {
            var keyValue = Tick & 0xFFFF_0000; // Mask first nibble which is used to derive a key

            var format = new StandardFormat('x', 8); // 0xABCD => 0000abcd

            return Utf8Formatter.TryFormat(keyValue, buffer, out var _, format);
        }
    }
}
