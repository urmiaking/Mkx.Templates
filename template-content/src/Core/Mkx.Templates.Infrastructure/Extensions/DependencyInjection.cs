using Mkx.Templates.Infrastructure.Factories;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Sdk.Server.Infrastructure.Identity;
using Mkx.Templates.Sdk.Shared.Extensions;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mkx.Templates.Infrastructure.Extensions;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructure(IConfiguration configuration)
        {
            services
                .AddStorage(configuration)
                .AddEntityFrameworkIdentity();

            services.AddScoped<IUserClaimsPrincipalFactory<AppUser>, AppUserClaimsPrincipalFactory>();

            services.DiscoverServices();

            return services;
        }

        internal IServiceCollection AddStorage(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("Default");

            if (string.IsNullOrEmpty(connectionString))
                throw new Exception("'Default' connection string is not set.");

            services.AddSqlServer<AppDbContext>(connectionString, options =>
                {
                    options.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                },
                dbContextOptions =>
                {
                    dbContextOptions.ConfigureWarnings(w =>
                        w.Ignore(SqlServerEventId.SavepointsDisabledBecauseOfMARS));
                });

            services.AddDataProtection()
                .PersistKeysToDbContext<AppDbContext>();

            return services;
        }

        internal IServiceCollection AddEntityFrameworkIdentity()
        {
            services.AddIdentity<AppUser, AppRole>(options =>
                {
                    options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
                })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders()
                .AddSignInManager<AppSignInManager<AppUser>>();

            return services;
        }
    }
}
