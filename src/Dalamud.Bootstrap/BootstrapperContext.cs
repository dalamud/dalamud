using System;
using System.Text.Json.Serialization;

namespace Dalamud.Bootstrap
{
    /// <summary>
    /// Carries additional informations for Dalamud bootstrapper.
    /// All propreties that is not nullable must be set before passing to bootstrapper.
    /// </summary>
    [Serializable]
    public record BootstrapperContext
    {
        /// <summary>
        /// A path to where dalamud binaries are located. (e.g. dalamud_boot.dll)
        /// </summary>
        [JsonPropertyName("dalamud_root")]
        public string DalamudRoot { get; init; } = null!;

        /// <summary>
        /// A path to where the profile is located. (e.g. directory that contains plugins/, data/)
        /// 
        /// If directory is empty then it will be assumed to be its "first launch".
        /// </summary>
        [JsonPropertyName("profile_root")]
        public string ProfileRoot { get; init; } = null!;
    }
}
