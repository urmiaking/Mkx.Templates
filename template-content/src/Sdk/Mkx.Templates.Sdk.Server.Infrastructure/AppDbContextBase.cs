using Mkx.Templates.Sdk.Server.Infrastructure.Configurations;
using Mkx.Templates.Sdk.Server.Infrastructure.Interceptors;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Bus;

namespace Mkx.Templates.Sdk.Server.Infrastructure;

public abstract class
    AppDbContextBase<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TUserPasskey>(
        DbContextOptions options, IServiceProvider serviceProvider)
    : IdentityDbContext<TUser, TRole, TKey, TUserClaim, TUserRole, TUserLogin, TRoleClaim, TUserToken, TUserPasskey>(options), IDataProtectionKeyContext
    where TUser : IdentityUser<TKey>
    where TRole : IdentityRole<TKey>
    where TKey : IEquatable<TKey>
    where TUserClaim : IdentityUserClaim<TKey>
    where TUserRole : IdentityUserRole<TKey>
    where TUserLogin : IdentityUserLogin<TKey>
    where TRoleClaim : IdentityRoleClaim<TKey>
    where TUserToken : IdentityUserToken<TKey>
    where TUserPasskey : IdentityUserPasskey<TKey>
{
    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var bus = serviceProvider.GetService<IBus>();

        if (bus != null)
            optionsBuilder.AddInterceptors(new DomainEventsInterceptor(bus));

        optionsBuilder.AddInterceptors(new PersianizerInterceptor());

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.SetupIdentityTables();
    }
}
