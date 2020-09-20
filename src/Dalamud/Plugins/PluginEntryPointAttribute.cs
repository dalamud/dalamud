using System;

namespace Dalamud.Plugins
{
    [AttributeUsage(AttributeTargets.Assembly)]
    public class PluginEntryPointAttribute : Attribute
    {
        public Type Type { get; init; }

        public PluginEntryPointAttribute(Type type)
        {
            Type = type;
        }
    }
}
