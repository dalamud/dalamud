using System;

namespace Dalamud.Plugins
{
    /// <summary>
    /// Sets when a plugin should be loaded by Dalamud
    /// </summary>
    public enum PluginLoadTime
    {
        /// <summary>
        /// Your plugin entry point will be called once Dalamud can init all of it's internal game interfaces, which has no guaranteed
        /// runtime outside of sometime this century. This is the default behaviour.
        /// </summary>
        WhenReady,
        
        /// <summary>
        /// Early load time is as soon as the game starts. This mean your plugin will be loaded at the earliest possible time, but this
        /// comes with some caveats.
        /// </summary>
        /// <remarks>
        /// Plugin DI is ignored for plugins (e.g, you get null game modules) that load early to avoid any delay before dalamud has attempted to init each module.
        /// Therefore, if you want dependencies, you will need to manually pull them out of the IOC container later.
        /// </remarks>
        Early,
    }

    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginLoadTimeAttribute : Attribute
    {
        public PluginLoadTime LoadAt { get; init; }

        public PluginLoadTimeAttribute(PluginLoadTime loadAt)
        {
            LoadAt = loadAt;
        }
    }
}
