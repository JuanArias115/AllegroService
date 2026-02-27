using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_StockMovements_QtyPositive", "\"Qty\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.Type)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Qty)
            .ConfigureQuantity();

        builder.Property(x => x.ReferenceType)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.GlampingId, x.ProductId });
        builder.HasIndex(x => new { x.GlampingId, x.LocationId });
        builder.HasIndex(x => new { x.ReferenceType, x.ReferenceId });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany(x => x.StockMovements)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
