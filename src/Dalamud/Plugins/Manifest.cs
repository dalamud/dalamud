using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dalamud.Plugins
{
    /// <summary>
    /// Description of the plugin that can be read without loading the code.
    /// </summary>
    public record Manifest
    {
        /// <summary>
        /// Unique identifier of the plugin.
        /// </summary>
        public string Id { get; init; }

        /// <summary>
        /// Path to the main dotnet assembly that contains Dalamud plugin type.
        /// </summary>
        public string Assembly { get; init; }

        /// <summary>
        /// Plugin name that is actually displayed on the screen.
        /// </summary>
        public string? DisplayName { get; init; }

        public string? PluginVersion { get; init; }

        /// <summary>
        /// Little flavour text about the plugin.
        /// </summary>
        public string? Description { get; init; }

        public string[]? Tags { get; init; }

        /// <summary>
        /// URL link to where detailed information can be found about this plugin.
        /// </summary>
        public string? Homepage { get; init; }

        /// <summary>
        /// Name of the person(or screenname; anything) who made this plugin.
        /// </summary>
        public string[]? Authors { get; init; }
    }
}
