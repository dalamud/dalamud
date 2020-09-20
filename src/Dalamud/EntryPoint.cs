using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Loader;
using System.Text;
using Serilog;

// Note thast
namespace Dalamud.Boot
{
    /// <summary>
    /// Entrypoint of Dalamud.
    /// </summary>
    public static class EntryPoint
    {
        public unsafe static int Initialize(nint args, int size)
        {
            Log.Logger = CreateLogger();
            Log.Information("Dalamud.Core loaded");
            
            var paramJson = Encoding.UTF8.GetString((byte*) args, size);
            Log.Verbose("param passed from dalamud_boot: {paramJson}", paramJson);

            return 0;
        }

        private static ILogger CreateLogger() => new LoggerConfiguration()
            .WriteTo.Console()
            .MinimumLevel.Verbose()
            .CreateLogger();
    }
}
