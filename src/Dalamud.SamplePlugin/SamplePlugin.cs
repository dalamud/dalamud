using System;
using Dalamud.Game;
using Dalamud.IOC;
using Dalamud.Plugins;
using Dalamud.SamplePlugin;

[assembly: PluginEntryPoint(typeof(SamplePlugin))]
[assembly: PluginLoadTime(PluginLoadTime.WhenReady)]

namespace Dalamud.SamplePlugin
{
    public sealed class SamplePlugin : PluginBase
    {
        private readonly ITestInterface m_testInterface;

        public SamplePlugin(
            [RequiredVersion("1.0")] ITestInterface testInterface 
        )
        {
            // your constructor only gets called in the event that dalamud can satisfy all of its dependencies through the constructor

            m_testInterface = testInterface;
            
            // register some api
            CallGate.RegisterAction( nameof( DoSomethingCool ), DoSomethingCool );
        }

        public override void OnPaint()
        {
            // draw gui here
        }

        private unsafe void DoSomethingCool()
        {
            *(byte*)0 = 0;
        }
    }
}
