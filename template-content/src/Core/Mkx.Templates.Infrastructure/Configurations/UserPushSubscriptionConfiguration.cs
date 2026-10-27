using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mkx.Templates.Domain.Notifications;
using Mkx.Templates.Sdk.Server.Domain.Identity;

namespace Mkx.Templates.Infrastructure.Configurations;

internal sealed class UserPushSubscriptionConfiguration : IEntityTypeConfiguration<UserPushSubscription>
{
    public void Configure(EntityTypeBuilder<UserPushSubscription> builder)
    {
        builder.ToTable("UserPushSubscriptions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Endpoint).HasMaxLength(2048).IsRequired();
        builder.Property(x => x.EndpointHash).HasMaxLength(64).IsRequired().IsUnicode(false);
        builder.Property(x => x.P256dh).HasMaxLength(256).IsRequired();
        builder.Property(x => x.Auth).HasMaxLength(128).IsRequired();
        builder.Property(x => x.DeviceName).HasMaxLength(120);
        builder.HasIndex(x => x.EndpointHash).IsUnique();
        builder.HasIndex(x => x.UserId);
        builder.HasOne<AppUser>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
    }
}
