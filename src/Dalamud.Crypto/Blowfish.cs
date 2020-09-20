using System;
using System.Runtime.CompilerServices;

namespace Dalamud.Crypto
{
    /// <summary>
    /// This class implements essence of Blowfish algorithm.
    /// </summary>
    public abstract partial class Blowfish
    {
        private BlowfishState m_state;

        /// <summary>
        /// Creates Blowfish instance.
        /// </summary>
        /// <param name="key">An encryption key. Note that its length will be verified and assumed to be correct. Use HasValidKeyLength() to verify key length.</param>
        protected Blowfish(ReadOnlySpan<byte> key)
        {
            InitializeState();
            InitializeKey(key);
        }

        /// <summary>
        /// Copies initial Blowfish state (P and S boxes) to the current instance. 
        /// </summary>
        private void InitializeState()
        {
            const uint PSizeInBytes = PSize * sizeof(uint);
            const uint SSizeInBytes = SSize * sizeof(uint);
            
            unsafe
            {
                fixed (BlowfishState* pState = &m_state)
                fixed (uint* pPInit = PInit)
                fixed (uint* pS0Init = S0Init)
                fixed (uint* pS1Init = S1Init)
                fixed (uint* pS2Init = S2Init)
                fixed (uint* pS3Init = S3Init)
                {
                    // assume compiler can take care of alignment required
                    Unsafe.CopyBlock(pState->P, pPInit, PSizeInBytes);
                    Unsafe.CopyBlock(pState->S0, pS0Init, SSizeInBytes);
                    Unsafe.CopyBlock(pState->S1, pS1Init, SSizeInBytes);
                    Unsafe.CopyBlock(pState->S2, pS2Init, SSizeInBytes);
                    Unsafe.CopyBlock(pState->S3, pS3Init, SSizeInBytes);
                }
            }
        }

        /// <summary>
        /// Process P and S boxes with given key.
        /// </summary>
        /// <remarks>
        /// This is expensive operation and what makes possible to extend Blowfish to BCrypt algorithm.
        /// </remarks>
        private void InitializeKey(ReadOnlySpan<byte> key)
        {
            // Initialize P and S-boxes with key
            unsafe
            {
                var keyPos = 0;

                for (var i = 0; i < PSize; i++)
                {
                    var val = 0u;

                    // inlined wrapping u32 (be)
                    // eg. key = { 12 34 56 78 AB CD EF GH HI JK }
                    // => {0x12345678, 0xABCDEFGH, 0xHIJK1234, 0x5678ABCD, ..}
                    for (var j = 0; j < 4; j++)
                    {
                        // wrap to the start when we reached the end.
                        if (keyPos >= key.Length)
                        {
                            keyPos = 0;
                        }

                        val = (val << 8) | key[keyPos++];
                    }

                    m_state.P[i] ^= val;
                }

                var xl = 0u;
                var xr = 0u;

                for (var i = 0; i < PSize; i += 2)
                {
                    (xl, xr) = EncryptBlock(xl, xr);

                    m_state.P[i] = xl;
                    m_state.P[i + 1] = xr;
                }

                for (var i = 0; i < SSize; i += 2)
                {
                    (xl, xr) = EncryptBlock(xl, xr);

                    m_state.S0[i] = xl;
                    m_state.S0[i + 1] = xr;
                }

                for (var i = 0; i < SSize; i += 2)
                {
                    (xl, xr) = EncryptBlock(xl, xr);

                    m_state.S1[i] = xl;
                    m_state.S1[i + 1] = xr;
                }

                for (var i = 0; i < SSize; i += 2)
                {
                    (xl, xr) = EncryptBlock(xl, xr);

                    m_state.S2[i] = xl;
                    m_state.S2[i + 1] = xr;
                }

                for (var i = 0; i < SSize; i += 2)
                {
                    (xl, xr) = EncryptBlock(xl, xr);

                    m_state.S3[i] = xl;
                    m_state.S3[i + 1] = xr;
                }
            }
        }

        /// <summary>
        /// Encrypts a block.
        /// </summary>
        /// <param name="xl">A left side of the block.</param>
        /// <param name="xr">A right side of the block.</param>
        protected (uint, uint) EncryptBlock(uint xl, uint xr)
        {
            unsafe
            {
                // https://en.wikipedia.org/wiki/Feistel_cipher#Construction_details
                for (var i = 0; i < Rounds; i += 2)
                {
                    xl ^= m_state.P[i];
                    xr ^= Round(xl);
                    xr ^= m_state.P[i + 1];
                    xl ^= Round(xr);
                }

                xl ^= m_state.P[16];
                xr ^= m_state.P[17];

                // swap(L, R)
                var temp = xl;
                xl = xr;
                xr = temp;

                return (xl, xr);
            }
        }

        /// <summary>
        /// Decrypts a block.
        /// </summary>
        /// <param name="xl">A left side of the block.</param>
        /// <param name="xr">A right side of the blick.</param>
        protected (uint, uint) DecryptBlock(uint xl, uint xr)
        {
            unsafe
            {
                // https://en.wikipedia.org/wiki/Feistel_cipher#Construction_details
                for (var i = Rounds; i > 0; i -= 2)
                {
                    xl ^= m_state.P[i + 1];
                    xr ^= Round(xl);
                    xr ^= m_state.P[i];
                    xl ^= Round(xr);
                }

                xl ^= m_state.P[1];
                xr ^= m_state.P[0];

                // swap(L, R);
                var temp = xl;
                xl = xr;
                xr = temp;

                return (xl, xr);
            }
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining /* | MethodImplOptions.AggressiveOptimization */)]
        private uint Round(uint x)
        {
            unsafe
            {
                // NOTE: this is still sub-optimal
                return unchecked
                (
                    ((m_state.S0[x >> 24] + m_state.S1[(byte)(x >> 16)]) ^ m_state.S2[(byte)(x >> 8)]) + m_state.S3[(byte)x]
                );
            }
        }

        /// <summary>
        /// Returns true if length of the key is supported, false otherwise.
        /// </summary>
        protected static bool HasValidKeyLength(uint length) => length switch
        {
            // valid size: 32~448 bits
            _ when (length >= 4 && length <= 56) => true,
            _ => false,
        };

        /// <summary>
        /// Returns true if length of the plaintext is correctly aligned to block size, false otherwise.
        /// </summary>
        protected static bool HasValidPlainTextLength(uint length) => length % BlockSize == 0;

        /// <summary>
        /// Throws an exception when length of the plaintext is not aligned to required block size.
        /// </summary>
        protected static void CheckPlainTextLength(uint length)
        {
            if (!HasValidPlainTextLength(length))
            {
                throw new ArgumentException("A length of the buffer must be multiple of block size which is 8 bytes.", nameof(length));
            }
        }
    }
}
