using System;
using System.Text;

namespace Dalamud.Bootstrap
{
    struct CommandLineWriter
    {
        private readonly StringBuilder m_buffer;

        public CommandLineWriter(StringBuilder buffer) => m_buffer = buffer;

        public void WriteDelimiter()
        {
            m_buffer.Append(' ');
        }

        public void WriteArgument(string argument)
        {
            // n backslashes **not** followed by double quote (e.g. \, \\, \\\, ...) produces n backslashes literally (e.g. \, \\, \\\, ...respectively)
            // 2n backslashes followed by double quote (e.g. \\", \\\\", \\\\\\", ...) produces n backslashes (same as above) and toggles quote mode
            // (2n)+1 backslashes followed by double quote (e.g. \", \\\", \\\\\" ...) produces n backslashes (same as above) and does not toggle quote mode
            //
            // but for the fuck of implementation sake we'll just quote everything (there's no downside anyway)

            m_buffer.Append('"'); // begins quote mode

            // append actual contents
            var backslashes = 0;
            foreach (var chr in argument)
            {
                switch (chr)
                {
                    case '\\':
                        backslashes += 1;
                        break;
                    case '"':
                        m_buffer.Append('\\', backslashes + 1); // put n+1 more quotes to produce 2n+1 backslashes (without toggling quote mode)
                        backslashes = 0;
                        break;
                    default:
                        backslashes = 0; // not followed by double quote
                        break;
                }

                m_buffer.Append(chr); // push code unit
            }

            // ends quote quote
            m_buffer.Append('\\', backslashes); // put n more backslashes to produce 2n backslashes
            m_buffer.Append('"'); // 2n backslashes followed by the double-quote
        }   
    }
}
