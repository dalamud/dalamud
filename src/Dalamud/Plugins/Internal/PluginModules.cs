using System;
using System.Collections.Generic;

namespace Dalamud.Plugins.Internal
{
    class PluginModules
    {
        Dictionary<Type, PluginBase> m_stuff = new(); // TODO

        public void AddModule<T>() where T : PluginBase, new()
        {
            throw new NotImplementedException();
        }

        public void AddModule<T>(T module) where T : PluginBase
        {
            m_stuff[typeof(T)] = module;
            throw new NotImplementedException("TODO");
        }
    }
}
