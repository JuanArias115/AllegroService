using AllegroService.Domain.Entities;
using AllegroService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class FolioConfiguration : IEntityTypeConfiguration<Folio>
{
    public void Configure(EntityTypeBuilder<Folio> builder)
    {
        builder.ToTable("Folios");

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.OpenedAt)
            .IsRequired();

        builder.Property(x => x.ClosedAt);

        builder.HasIndex(x => x.StayId);

        builder.HasIndex(x => x.StayId)
            .IsUnique()
            .HasFilter($"\"Status\" = {(int)FolioStatus.Open}");

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.Folios)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Stay)
            .WithMany(x => x.Folios)
            .HasForeignKey(x => x.StayId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
