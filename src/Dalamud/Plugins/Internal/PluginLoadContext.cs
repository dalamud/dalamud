using System;
using System.Reflection;
using System.Runtime.Loader;

namespace Dalamud.Plugins.Internal
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        private readonly AssemblyDependencyResolver m_resolver;

        public PluginLoadContext(string assemblyPath) : base(isCollectible: true)
        {
            m_resolver = new AssemblyDependencyResolver(assemblyPath);
        }

        protected override Assembly? Load(AssemblyName name)
        {
            var assemblyPath = m_resolver.ResolveAssemblyToPath(name);
            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            var dllPath = m_resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            if (dllPath != null)
            {
                return LoadUnmanagedDllFromPath(dllPath);
            }

            return IntPtr.Zero;
        }
    }
}
