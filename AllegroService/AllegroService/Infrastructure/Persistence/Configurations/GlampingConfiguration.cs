using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class GlampingConfiguration : IEntityTypeConfiguration<Glamping>
{
    public void Configure(EntityTypeBuilder<Glamping> builder)
    {
        builder.ToTable("Glampings");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Address)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.Timezone)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.ConfigureAudit();
    }
}
