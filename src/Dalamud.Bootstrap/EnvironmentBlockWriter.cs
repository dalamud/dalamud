using System.Text;

namespace Dalamud.Bootstrap
{
    struct EnvironmentBlockWriter
    {
        private readonly StringBuilder m_buffer;

        public EnvironmentBlockWriter(StringBuilder buffer) => m_buffer = buffer;

        public void WriteEntry(string key, string value)
        {
            m_buffer.Append(key);
            m_buffer.Append('=');
            m_buffer.Append(value);
            m_buffer.Append('\0'); // each entry is nul terminated
        }

        public void WriteTerminator()
        {
            m_buffer.Append('\0'); // denotes there are no elements after this
        }
    }
}
