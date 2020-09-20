using System.IO;
using System.Text.Json;

namespace Dalamud.Extensions
{
    public static class ObjectExtensions
    {
        public static void SerializeToFile( this object obj, string path )
        {
            var data = JsonSerializer.Serialize( obj, new JsonSerializerOptions
            {
                WriteIndented = true
            });
            
            File.WriteAllText( path, data );
        }
    }
}
