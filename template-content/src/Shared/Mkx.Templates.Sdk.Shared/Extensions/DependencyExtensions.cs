using Mkx.Templates.Sdk.Shared.Attributes;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Reflection;

namespace Mkx.Templates.Sdk.Shared.Extensions;

public static class DependencyExtensions
{
    /// <param name="services">Instance of the service collection</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Discovers services decorated by <see cref="SingletonServiceAttribute"/> or <see cref="ScopedServiceAttribute"/> from the given assembly.
        /// </summary>
        /// <param name="assembly">The target assembly to be discovered</param>
        /// <returns>Returns the service collection</returns>
        public IServiceCollection DiscoverServices(Assembly assembly)
        {
            services.DiscoverSingletonServices(assembly);
            services.DiscoverScopedServices(assembly);
            services.DiscoverTransientServices(assembly);

            return services;
        }

        /// <summary>
        /// Discovers services decorated by <see cref="SingletonServiceAttribute"/> or <see cref="ScopedServiceAttribute"/> from calling assembly.
        /// </summary>
        /// <returns>Returns the service collection</returns>
        public IServiceCollection DiscoverServices()
        {
            var assembly = Assembly.GetCallingAssembly();

            services.DiscoverServices(assembly);

            return services;
        }

        /// 
        private void DiscoverSingletonServices(Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var serviceAttrs = type.GetCustomAttributes(false)
                    .Where(x => x.GetType().FullName == typeof(SingletonServiceAttribute).FullName);

                foreach (var serviceAttr in serviceAttrs)
                {
                    var attributeType = serviceAttr.GetType();
                    var implementationType = type;
                    var baseTypeInterfaces = implementationType.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>();
                    var allInterfaces = implementationType.GetInterfaces();
                    var directInterfaces = allInterfaces
                        .Except(allInterfaces.SelectMany(t => t.GetInterfaces()))
                        .Except(baseTypeInterfaces);

                    if (attributeType.IsGenericType)
                    {
                        var serviceType = attributeType.GenericTypeArguments[0];

                        services.AddSingleton(serviceType, implementationType);
                    }
                    else if (implementationType.GetInterfaces().Any())
                    {
                        foreach (var interfaceType in directInterfaces)
                        {
                            services.AddSingleton(interfaceType, implementationType);
                        }
                    }
                    else
                    {
                        services.AddSingleton(implementationType);
                    }
                }
            }
        }

        /// 
        private void DiscoverScopedServices(Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var serviceAttrs = type.GetCustomAttributes(false)
                    .Where(x => x.GetType().FullName == typeof(ScopedServiceAttribute).FullName);

                foreach (var serviceAttr in serviceAttrs)
                {
                    var attributeType = serviceAttr.GetType();
                    var implementationType = type;
                    var baseTypeInterfaces = implementationType.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>();
                    var allInterfaces = implementationType.GetInterfaces();
                    var directInterfaces = allInterfaces
                        .Except(allInterfaces.SelectMany(t => t.GetInterfaces()))
                        .Except(baseTypeInterfaces);

                    if (attributeType.IsGenericType)
                    {
                        var serviceType = attributeType.GenericTypeArguments[0];

                        services.AddScoped(serviceType, implementationType);
                    }
                    else if (directInterfaces.Any())
                    {
                        foreach (var interfaceType in directInterfaces)
                        {
                            services.AddScoped(interfaceType, implementationType);
                        }
                    }
                    else
                    {
                        services.AddScoped(implementationType);
                    }
                }
            }
        }

        /// 
        private void DiscoverTransientServices(Assembly assembly)
        {
            var types = assembly.GetTypes();

            foreach (var type in types)
            {
                var serviceAttrs = type.GetCustomAttributes(false)
                    .Where(x => x.GetType().FullName == typeof(TransientServiceAttribute).FullName);

                foreach (var serviceAttr in serviceAttrs)
                {
                    var attributeType = serviceAttr.GetType();
                    var implementationType = type;
                    var baseTypeInterfaces = implementationType.BaseType?.GetInterfaces() ?? Enumerable.Empty<Type>();
                    var allInterfaces = implementationType.GetInterfaces();
                    var directInterfaces = allInterfaces
                        .Except(allInterfaces.SelectMany(t => t.GetInterfaces()))
                        .Except(baseTypeInterfaces);

                    if (attributeType.IsGenericType)
                    {
                        var serviceType = attributeType.GenericTypeArguments[0];

                        services.AddTransient(serviceType, implementationType);
                    }
                    else if (directInterfaces.Any())
                    {
                        foreach (var interfaceType in directInterfaces)
                        {
                            services.AddTransient(interfaceType, implementationType);
                        }
                    }
                    else
                    {
                        services.AddTransient(implementationType);
                    }
                }
            }
        }
    }
}