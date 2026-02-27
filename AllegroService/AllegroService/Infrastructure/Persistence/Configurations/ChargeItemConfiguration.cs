using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class ChargeItemConfiguration : IEntityTypeConfiguration<ChargeItem>
{
    public void Configure(EntityTypeBuilder<ChargeItem> builder)
    {
        builder.ToTable("ChargeItems", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_ChargeItems_TotalNonNegative", "\"Total\" >= 0");
            tableBuilder.HasCheckConstraint("CK_ChargeItems_QtyPositive", "\"Qty\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.Qty)
            .ConfigureQuantity();

        builder.Property(x => x.UnitPrice)
            .ConfigureMoney();

        builder.Property(x => x.Total)
            .ConfigureMoney();

        builder.HasIndex(x => new { x.GlampingId, x.ChargeId });
        builder.HasIndex(x => new { x.GlampingId, x.ProductId });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.ChargeItems)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Charge)
            .WithMany(x => x.Items)
            .HasForeignKey(x => x.ChargeId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.ChargeItems)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
