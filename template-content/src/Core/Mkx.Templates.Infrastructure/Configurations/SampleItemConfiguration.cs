using Mkx.Templates.Domain.SampleItemAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Mkx.Templates.Infrastructure.Configurations;

internal sealed class SampleItemConfiguration : IEntityTypeConfiguration<SampleItem>
{
    public void Configure(EntityTypeBuilder<SampleItem> builder)
    {
        builder.ToTable("Tests");

        builder.Property(x => x.Id)
            .HasConversion(x => x.Value,
                id => new SampleItemId(id));

        builder.Property(x => x.Version).IsConcurrencyToken();

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}
