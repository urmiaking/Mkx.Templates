using Mkx.Templates.Sdk.Server.Domain.Identity;
using Mkx.Templates.Domain.Notifications;
using Mkx.Templates.Sdk.Server.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Mkx.Templates.Infrastructure;
 
public class AppDbContext(
    DbContextOptions<AppDbContext> options, IServiceProvider serviceProvider)
    : AppDbContextBase<AppUser, AppRole, Guid, AppUserClaim, AppUserRole, AppUserLogin, AppRoleClaim, AppUserToken, AppUserPasskey>(
        options, serviceProvider)
{
    public DbSet<UserPushSubscription> UserPushSubscriptions => Set<UserPushSubscription>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        base.OnModelCreating(builder);
    }
}