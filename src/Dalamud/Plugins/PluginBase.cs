using System;
using System.Threading.Tasks;

namespace Dalamud.Plugins
{
    public abstract class PluginBase : IAsyncDisposable
    {
        protected CallGate CallGate { get; } = new();

        public dynamic PluginInterface => CallGate;
        
        public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public virtual void OnPaint()
        {
        }
    }
}
