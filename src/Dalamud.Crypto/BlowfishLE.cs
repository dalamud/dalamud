using System;
using System.Buffers.Binary;
using System.Runtime.CompilerServices;

namespace Dalamud.Crypto
{
    /// <summary>
    /// This class implements Blowfish algorithm with a twist of little endian for read/write primitives. (See remarks)
    /// </summary>
    /// <remarks>
    /// While it is not typical way to use little endian for reading/writing u32, this is needed to match the behavior of what FFXIV does.
    /// </remarks>
    public sealed partial class BlowfishLE : Blowfish
    {
        internal BlowfishLE(ReadOnlySpan<byte> key) : base(key) { }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public static BlowfishLE Create(ReadOnlySpan<byte> key)
        {
            if (!HasValidKeyLength((uint) key.Length))
            {
                throw new ArgumentException("Only key between 32 to 448 bits long is supported.", nameof(key));
            }

            return new BlowfishLE(key);
        }

        /// <summary>
        /// Checks if block can be read without misalignment penalty.
        /// </summary>
        /// <remarks>
        /// Returns true if the address of the buffer is aligned to 4 bytes. (half of the block size for feistel cipher)
        /// </remarks>
        private static unsafe bool IsBufferAligned(byte* pBuffer) => ((nint) pBuffer % sizeof(uint)) == 0;

        /// <summary>
        /// Encrypt blocks in-place.
        /// </summary>
        public void Encrypt(Span<byte> buffer)
        {
            CheckPlainTextLength((uint) buffer.Length);
            
            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    if (IsBufferAligned(pBuffer))
                    {
                        EncryptAligned(pBuffer, buffer.Length);
                    }
                    else
                    {
                        EncryptUnaligned(pBuffer, buffer.Length);
                    }
                }
            }
        }

        private unsafe void EncryptAligned(byte* pBuffer, int length)
        {
            unsafe
            {
                for (var pBufferEnd = pBuffer + length; pBuffer < pBufferEnd; pBuffer += 8)
                {
                    var xl = Unsafe.Read<uint>(pBuffer);
                    var xr = Unsafe.Read<uint>(pBuffer + 4);

                    // will be elided by JIT
                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    (xl, xr) = EncryptBlock(xl, xr);

                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    Unsafe.Write(pBuffer, xl);
                    Unsafe.Write(pBuffer + 4, xr);
                }
            }
        }

        private unsafe void EncryptUnaligned(byte* pBuffer, int length)
        {
            unsafe
            {
                for (var pBufferEnd = pBuffer + length; pBuffer < pBufferEnd; pBuffer += 8)
                {
                    var xl = Unsafe.ReadUnaligned<uint>(pBuffer);
                    var xr = Unsafe.ReadUnaligned<uint>(pBuffer + 4);

                    // will be elided by JIT
                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    (xl, xr) = EncryptBlock(xl, xr);

                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    Unsafe.WriteUnaligned(pBuffer, xl);
                    Unsafe.WriteUnaligned(pBuffer + 4, xr);
                }
            }
        }

        /// <summary>
        /// Decrypt blocks in-place.
        /// </summary>
        public void Decrypt(Span<byte> buffer)
        {
            CheckPlainTextLength((uint) buffer.Length);

            unsafe
            {
                fixed (byte* pBuffer = buffer)
                {
                    if (IsBufferAligned(pBuffer))
                    {
                        DecryptAligned(pBuffer, buffer.Length);
                    }
                    else
                    {
                        DecryptUnaligned(pBuffer, buffer.Length);
                    }
                }
            }
        }

        private unsafe void DecryptAligned(byte* pBuffer, int length)
        {
            unsafe
            {
                for (var pBufferEnd = pBuffer + length; pBuffer < pBufferEnd; pBuffer += 8)
                {
                    var xl = Unsafe.Read<uint>(pBuffer);
                    var xr = Unsafe.Read<uint>(pBuffer + 4);

                    // will be elided by JIT
                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    (xl, xr) = DecryptBlock(xl, xr);

                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    Unsafe.Write(pBuffer, xl);
                    Unsafe.Write(pBuffer + 4, xr);
                }
            }
        }

        private unsafe void DecryptUnaligned(byte* pBuffer, int length)
        {
            unsafe
            {
                for (var pBufferEnd = pBuffer + length; pBuffer < pBufferEnd; pBuffer += 8)
                {
                    var xl = Unsafe.ReadUnaligned<uint>(pBuffer);
                    var xr = Unsafe.ReadUnaligned<uint>(pBuffer + 4);

                    // will be elided by JIT
                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    (xl, xr) = DecryptBlock(xl, xr);

                    if (!BitConverter.IsLittleEndian)
                    {
                        xl = BinaryPrimitives.ReverseEndianness(xl);
                        xr = BinaryPrimitives.ReverseEndianness(xr);
                    }

                    Unsafe.WriteUnaligned(pBuffer, xl);
                    Unsafe.WriteUnaligned(pBuffer + 4, xr);
                }
            }
        }
    }
}
