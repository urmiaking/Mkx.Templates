using Mkx.Templates.Sdk.Server.Application.Abstractions;
using Mkx.Templates.Sdk.Server.Application.Services;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Mkx.Templates.Sdk.Server.Application.Extensions;

public static class DependencyInjection
{
    /// <param name="services">The IServiceCollection to add the user context accessor to.</param>
    extension(IServiceCollection services)
    {
        /// <summary>
        /// Registers the user context accessor service, enabling access to the current user's context via dependency injection.
        /// </summary>
        /// <returns>The IServiceCollection for chaining.</returns>
        public IServiceCollection AddUserContext()
        {
            services.AddHttpContextAccessor();
            services.AddScoped<IUserContext, UserContext>();

            return services;
        }

        public IServiceCollection AddMapsterMappers()
        {
            var globalSettings = TypeAdapterConfig.GlobalSettings;

            globalSettings.Scan(Assembly.GetCallingAssembly());
            services.AddSingleton(globalSettings);
            services.AddScoped<IMapper, ServiceMapper>();

            return services;
        }
    }
}
