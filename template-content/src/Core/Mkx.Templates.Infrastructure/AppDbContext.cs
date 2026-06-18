using Mkx.Templates.Infrastructure.Configurations;
using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Sdk.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Mkx.Templates.Infrastructure;
 
public class AppDbContext(
    DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider)
    : AppDbContextBase<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken, AppUserPasskey>(
        options, serviceProvider)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(TestConfiguration).Assembly);

        base.OnModelCreating(builder);
    }
}
