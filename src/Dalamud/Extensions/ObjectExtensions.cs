using System.IO;
using System.Text.Json;

namespace Dalamud.Extensions
{
    public static class ObjectExtensions
    {
        public static void SerializeToFile( this object obj, string path, bool indented = true )
        {
            var data = JsonSerializer.Serialize( obj, new JsonSerializerOptions
            {
                WriteIndented = indented
            });
            
            File.WriteAllText( path, data );
        }
    }
}
