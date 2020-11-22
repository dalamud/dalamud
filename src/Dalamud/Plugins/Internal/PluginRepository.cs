using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using Serilog;

namespace Dalamud.Plugins.Internal
{
    internal record DiscoveredPlugin
    {
        public DirectoryInfo PluginDir { get; init; }
    
        public Manifest Manifest { get; init; }
    }
    
    internal class PluginRepository
    {
        private const string ManifestFileName = "manifest.json";

        /// <summary>
        /// Canonical path to repository.
        /// </summary>
        private readonly DirectoryInfo m_repositoryDirectory;

        public PluginRepository( DirectoryInfo directory ) => m_repositoryDirectory = directory;

        public List< DiscoveredPlugin > DiscoverManifests()
        {
            var manifests = new List< DiscoveredPlugin >();
            
            foreach( var pluginDirectory in m_repositoryDirectory.GetDirectories() )
            {
                var manifestPath = Path.Join( pluginDirectory.FullName, ManifestFileName );

                if( !File.Exists( manifestPath ) )
                {
                    Log.Error(
                        "skipping plugin directory {PluginDirectory} - no manifest.json found",
                        pluginDirectory.FullName
                    );
                    continue;
                }

                // todo: error handling
                var manifest = JsonSerializer.Deserialize< Manifest >( File.ReadAllText( manifestPath ) );

                // validate that the assembly exists at least
                var assemblyPath = Path.Join( pluginDirectory.FullName, manifest.Assembly );
                if( !File.Exists( assemblyPath ) )
                {
                    Log.Error(
                        "skipping plugin, unable to find assembly: {AssemblyPath}",
                        assemblyPath
                    );
                    continue;
                }

                manifests.Add( new DiscoveredPlugin
                {
                    PluginDir = pluginDirectory,
                    Manifest = manifest
                });
                
            }
            
            return manifests;
        }
    }
}
