using Dalamud.IOC;
using Serilog;

namespace Dalamud.Game
{
    [DependencyVersion( "1.0" )]
    public interface ITestInterface
    {
        void Hello();
    }

    public class TestInterface : ITestInterface
    {
        public void Hello()
        {
            Log.Information( "hello world" );
        }
    }
}
