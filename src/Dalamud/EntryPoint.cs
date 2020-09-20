using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

// Note thast
namespace Dalamud.Boot
{
    /// <summary>
    /// Entrypoint of Dalamud.
    /// </summary>
    public static class EntryPoint
    {
        public static unsafe int Initialize(nint args, int size)
        {
            Log.Logger = CreateLogger();
            Log.Information("Dalamud.Core loaded");
            
            var paramJson = Encoding.UTF8.GetString((byte*) args, size);
            Log.Verbose("param passed from dalamud_boot: {paramJson}", paramJson);

            var data = JsonSerializer.Deserialize< DalamudInitParams >( paramJson );
            
            var dalamud = new DalamudCore( data );

            return 0;
        }

        private static ILogger CreateLogger() => new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger();
    }
}
