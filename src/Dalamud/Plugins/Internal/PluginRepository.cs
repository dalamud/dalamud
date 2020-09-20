using System.IO;

namespace Dalamud.Plugins.Internal
{
    class PluginRepository
    {
        private const string ManifestFileName = "manifest.json";

        /// <summary>
        /// Canonical path to repository.
        /// </summary>
        private readonly string m_repositoryDirectory;

        public PluginRepository(string directory) => 
            m_repositoryDirectory = Path.GetFullPath(directory);
        
        public void Discover()
        {
            foreach (var pluginDirectory in Directory.GetDirectories(m_repositoryDirectory))
            {
                var manifestPath = Path.Join(pluginDirectory, ManifestFileName);

                if (File.Exists(manifestPath))
                {
                    // read manifest..
                }
                
                // TODO
                // 1. look for manifest.json
                // 2. openRead()
                // 3. parse to json
                //  - if one of them is invalid... what to do
                // 4. return all

                // NOTE: it should not manage plugin loading here
            }
        }
    }
}
