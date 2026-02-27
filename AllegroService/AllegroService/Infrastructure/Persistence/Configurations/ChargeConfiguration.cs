using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class ChargeConfiguration : IEntityTypeConfiguration<Charge>
{
    public void Configure(EntityTypeBuilder<Charge> builder)
    {
        builder.ToTable("Charges", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Charges_TotalNonNegative", "\"Total\" >= 0");
            tableBuilder.HasCheckConstraint("CK_Charges_QtyPositive", "\"Qty\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.Source)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(x => x.Qty)
            .ConfigureQuantity();

        builder.Property(x => x.UnitPrice)
            .ConfigureMoney();

        builder.Property(x => x.Total)
            .ConfigureMoney();

        builder.Property(x => x.MetadataJson)
            .HasColumnType("jsonb");

        builder.HasIndex(x => new { x.GlampingId, x.FolioId });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.Charges)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Folio)
            .WithMany(x => x.Charges)
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
