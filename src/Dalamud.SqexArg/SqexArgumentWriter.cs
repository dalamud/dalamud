using System;
using System.Buffers;
using System.Text;
using Dalamud.Crypto;

namespace Dalamud.SqexArg
{
    public struct SqexArgumentWriter
    {
        private const string BeginKey = " /";
        private const string BeginValue = " =";
        private const string EscapedSpace = "  ";

        private readonly StringBuilder m_buffer;

        public SqexArgumentWriter(StringBuilder buffer) => m_buffer = buffer;

        /// <summary>
        /// Pushes an argument string at the end.
        /// </summary>
        public void Append(string key, string value)
        {
            var escapedKey = Escape(key);
            var escapedValue = Escape(value);

            m_buffer.Append(BeginKey);
            m_buffer.Append(escapedKey);
            m_buffer.Append(BeginValue);
            m_buffer.Append(escapedValue);
        }

        public string Build(Key key)
        {
            // https://xiv.dev/sqexarg
            // tl;dr;
            // 1. Build the utf8 plaintext
            // 2. Encrypt that UTF-8 plaintext with given key (BlowfishLE/ECB)
            // 3. Encode that ciphertext to the url safe variant of base64 string.
            // 4. format the string with the correspoding checksum

            var plainText = m_buffer.ToString();
            
            var utf8TextLength = Encoding.UTF8.GetByteCount(plainText);

            var cipherTextLength = (utf8TextLength + 7) & (~0b111); // align to the size of blowfish block
            var cipherText = new byte[cipherTextLength];

            unsafe
            {
                fixed (byte* pCipherText = cipherText)
                fixed (char* pPlainText = plainText)
                {
                    Encoding.UTF8.GetBytes(pPlainText, plainText.Length, pCipherText, cipherText.Length);
                }
            }

            Span<byte> keyBytes = stackalloc byte[8];
            key.TryFormat(keyBytes);

            var cipher = BlowfishLE.Create(keyBytes);
            cipher.Encrypt(cipherText);

            // https://base64.guru/standards/base64url
            var base64Text = Convert.ToBase64String(cipherText)
                .Replace('+', '-')
                .Replace('/', '_');

            var checksum = key.Checksum;

            return $"//**sqex0003{base64Text}{checksum}**//";
        }

        private static string Escape(string value)
        {
            return value.Replace(" ", EscapedSpace);
        }
    }
}
