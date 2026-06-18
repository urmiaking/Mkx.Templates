using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mkx.Templates.Infrastructure;
using Mkx.Templates.Sdk.Server.Application.Abstractions;
using Mkx.Templates.Sdk.Server.Application.Extensions;
using Mkx.Templates.Sdk.Server.Application.Services;
using Mkx.Templates.Sdk.Shared.Extensions;

namespace Mkx.Templates.Application.Extensions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddApplication(IConfiguration configuration)
        {
            services.AddMapsterMappers();
            services.AddUserContext();
            services.AddScoped<ITransactionContext, TransactionContext<AppDbContext>>();
            services.DiscoverServices();

            return services;
        }
    }
}
