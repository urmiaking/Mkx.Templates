using Mkx.Templates.Sdk.Server.Infrastructure.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Extensions;

public static class WebApplicationExtensions
{
    extension(WebApplication app)
    {
        public WebApplication ApplyDatabaseMigrations<TContext>()
            where TContext : DbContext
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            var pendingMigrations = context.Database.GetPendingMigrations().ToList();

            if (pendingMigrations.Count > 0)
            {
                context.Database.Migrate();
            }

            return app;
        }

        public async Task SeedDatabaseAsync(CancellationToken cancellationToken = default)
        {
            using var scope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope();

            foreach (var service in scope.ServiceProvider.GetServices<IDbSeeder>().OrderBy(x => x.Order))
                await service.SeedAsync(cancellationToken);
        }
    }
}
