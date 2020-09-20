using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Dalamud.Plugins.Internal;

namespace Dalamud.Plugins
{
    /// <summary>
    /// Instance of plugin TODO
    /// </summary>
    class Plugin
    {
        private PluginLoadContext LoadContext { get; init; } = null!;

        private PluginBase Instance { get; init; } = null!;

        public Manifest Manifest { get; init; } = null!;

        private Plugin() { }

        private static async Task<Manifest> LoadManifestAsync(string path, CancellationToken cancellationToken = default)
        {
            using var manifestFile = File.OpenRead(path);

            var manifest = await JsonSerializer.DeserializeAsync<Manifest>(manifestFile, new JsonSerializerOptions
            {
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip,
                PropertyNameCaseInsensitive = true,
            }, cancellationToken);

            return manifest!;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pluginPath"></param>
        public static async Task<Plugin> LoadPlugin(string pluginPath)
        {
            throw new NotImplementedException();
        }
    }
}
