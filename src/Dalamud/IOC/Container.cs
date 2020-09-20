using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Dalamud.IOC.Internal;
using Serilog;

namespace Dalamud.IOC
{
    /// <summary>
    /// A simple singleton-only IOC container that provides (optional) version-based dependency resolution
    /// </summary>
    public class Container : IServiceProvider
    {
        private readonly Dictionary< Type, ObjectInstance > _objectInstances = new ();

        /// <summary>
        /// Register a singleton object of any type into the current IOC container
        /// </summary>
        /// <param name="instance">The existing instance to register in the container</param>
        /// <typeparam name="T">The interface to register</typeparam>
        public void RegisterSingleton< T >( T instance )
        {
            _objectInstances[ typeof( T ) ] = new( instance );
        }

        /// <summary>
        /// Register a singleton object of any type and implementing interface into the current IOC container
        /// </summary>
        /// <param name="impl"></param>
        /// <typeparam name="TInterface"></typeparam>
        /// <typeparam name="TImpl"></typeparam>
        public void RegisterSingleton< TInterface, TImpl >( TImpl impl )
        {
            _objectInstances[ typeof( TInterface ) ] = new( impl );
        }

        /// <summary>
        /// Attempt to find a ctor where we have a service registered for each of its parameters
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        private ConstructorInfo? FindApplicableCtor< T >() where T : class
        {
            var type = typeof( T );
            var ctors = type.GetConstructors( BindingFlags.Public | BindingFlags.Instance );

            // todo: this is a bit shit and is more of a first pass for now
            foreach( var ctor in ctors )
            {
                var @params = ctor.GetParameters();

                var failed = @params.Any( p => !_objectInstances.ContainsKey( p.ParameterType ) );

                if( !failed )
                {
                    return ctor;
                }
            }

            return null;
        }

        /// <summary>
        /// Create an object
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public T? Create< T >() where T : class
        {
            var type = typeof( T );
            
            var ctor = FindApplicableCtor< T >();
            if( ctor == null )
            {
                Log.Error(
                    "failed to create {TypeName}, unable to find any services to satisfy the dependencies in the ctor",
                    type.FullName 
                );

                return null;
            }
            
            // validate dependency versions (if they exist)
            var @params = ctor.GetParameters().Select( p =>
            {
                var attr = p.GetCustomAttribute( typeof( RequiredVersionAttribute ) ) as RequiredVersionAttribute;

                return new
                {
                    p.ParameterType,
                    RequiredVersion = attr
                };
            });

            var versionCheck = @params.Any( p =>
            {
                var declVersion = p.ParameterType.GetCustomAttribute( typeof( DependencyVersionAttribute ) ) as DependencyVersionAttribute;
                
                // if there's no requested/required version, just ignore it
                if( p.RequiredVersion == null || declVersion == null )
                {
                    return true;
                }

                if( declVersion.Version == p.RequiredVersion.Version )
                {
                    return true;
                }

                Log.Error(
                    "requested version: {ReqVersion} does not match the impl version: {ImplVersion} for param type {ParamType}",
                    p.RequiredVersion.Version,
                    declVersion.Version,
                    p.ParameterType.FullName
                );
                
                return false;
            } );

            if( !versionCheck )
            {
                Log.Error(
                    "failed to create {TypeName}, a RequestedVersion could not be satisfied",
                    type.FullName 
                );

                return null;
            }

            var resolvedParams = @params.Select( p => GetService( p.ParameterType ) ).ToArray();

            return Activator.CreateInstance( type, args: resolvedParams ) as T;
        }

        public object? GetService( Type serviceType )
        {
            return _objectInstances.TryGetValue( serviceType, out var service ) ? service.Instance : null;
        }
    }
}
