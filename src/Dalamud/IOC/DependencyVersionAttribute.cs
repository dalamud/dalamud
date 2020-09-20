using System;

namespace Dalamud.IOC
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface)]
    internal class DependencyVersionAttribute : Attribute
    {
        public readonly Version Version;

        public DependencyVersionAttribute( string version ) => ( Version ) = new( version );
    }
}
