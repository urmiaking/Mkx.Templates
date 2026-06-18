using Mkx.Templates.Sdk.Server.Domain.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mkx.Templates.Sdk.Server.Infrastructure.Configurations;

internal class AppUserPasskeyConfiguration : IEntityTypeConfiguration<AppUserPasskey>
{
    public void Configure(EntityTypeBuilder<AppUserPasskey> builder)
    {
        builder.ToTable("Passkeys");

        builder.HasKey(x => x.CredentialId);

        builder.HasOne(x => x.User)
            .WithMany(x => x.Passkeys)
            .HasForeignKey(ut => ut.UserId)
            .IsRequired();

        builder.OwnsOne(x => x.Data, b =>
        {
            b.ToJson();
        });
    }
}
