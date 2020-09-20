using System;
using System.Threading.Tasks;

namespace Dalamud.Plugins
{
    public abstract class PluginBase : IAsyncDisposable
    {
        public virtual ValueTask DisposeAsync() => ValueTask.CompletedTask;

        public virtual void OnPaint()
        {
        }
    }
}
