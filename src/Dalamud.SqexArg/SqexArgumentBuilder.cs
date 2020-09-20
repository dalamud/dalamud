using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dalamud.SqexArg
{
    public class SqexArgumentBuilder
    {
        private readonly Dictionary<string, string> m_arguments = new();

        public SqexArgumentBuilder Add(string key, string value)
        {
            m_arguments.Add(key, value);

            return this;
        }

        public string Build(Key encryptionKey)
        {
            var buffer = new StringBuilder();
            var writer = new SqexArgumentWriter(buffer);
            
            var hasTArg = false;

            foreach (var entry in m_arguments)
            {
                var key = entry.Key;
                var value = entry.Value;

                writer.Append(key, value);
                
                if (key == "T")
                    hasTArg = true;
            }
            
            if (!hasTArg)
                writer.Append("T", encryptionKey.Tick.ToString(CultureInfo.InvariantCulture));

            return writer.Build(encryptionKey);
        }
    }
}
