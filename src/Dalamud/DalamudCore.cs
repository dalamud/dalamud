using System.IO;
using System.Text.Json;
using Dalamud.Extensions;
using Serilog;

namespace Dalamud
{
    public record DalamudInitParams
    {
        public string BasePath { get; init; }
    }

    public class DalamudCore
    {
        private const string ConfigFileName = "config.json";
        
        private readonly DalamudInitParams m_initParams;

        private DalamudConfig m_config = null!;

        private DirectoryInfo m_baseDir = null!;
        private DirectoryInfo m_pluginsDir = null!;
        private DirectoryInfo m_pluginDataDir = null!;

        public DirectoryInfo PluginsDirectory => m_pluginsDir;

        public DalamudCore( DalamudInitParams initParams )
        {
            m_initParams = initParams;

            Log.Debug( "got basepath: {BasePath}", m_initParams.BasePath );
            
            SetupDataDirectory();
        }

        public void SetupDataDirectory()
        {
            m_baseDir = new( m_initParams.BasePath );
            if( !m_baseDir.Exists )
            {
                m_baseDir.Create();
            }

            var configFilePath = Path.Combine( m_initParams.BasePath, ConfigFileName );
            if( !File.Exists( configFilePath ) )
            {
                m_config = new();

                // write out initial config json
                m_config.SerializeToFile( Path.Combine( m_initParams.BasePath, "config.json" ) );
            }
            else
            {
                // todo: error handling
                
                // load existing cfg
                var data = File.ReadAllText( configFilePath );
                m_config = JsonSerializer.Deserialize< DalamudConfig >( data ) ?? new();
            }

            m_pluginsDir = new( Path.Combine( m_initParams.BasePath, m_config.PluginFolder ) );
            m_pluginDataDir = new( Path.Combine( m_initParams.BasePath, m_config.DataFolder ) );

            if( !m_pluginsDir.Exists )
            {
                m_pluginsDir.Create();
            }

            if( !m_pluginDataDir.Exists )
            {
                m_pluginDataDir.Create();
            }
        }
    }
}
