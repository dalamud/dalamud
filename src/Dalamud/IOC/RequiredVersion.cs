using System;

namespace Dalamud.IOC
{
    [AttributeUsage( AttributeTargets.Parameter )]
    public class RequiredVersion : Attribute
    {
        private Version m_version;

        public RequiredVersion( string version ) => ( m_version ) = new( version );
    }
}
