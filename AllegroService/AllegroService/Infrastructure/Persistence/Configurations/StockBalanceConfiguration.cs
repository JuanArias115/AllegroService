using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("StockBalances");

        builder.HasKey(x => new { x.ProductId, x.LocationId });

        builder.ConfigureTenantScope();

        builder.Property(x => x.QtyOnHand)
            .ConfigureQuantity();

        builder.HasIndex(x => new { x.GlampingId, x.ProductId });
        builder.HasIndex(x => new { x.GlampingId, x.LocationId });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.StockBalances)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Product)
            .WithMany(x => x.StockBalances)
            .HasForeignKey(x => x.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Location)
            .WithMany(x => x.StockBalances)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
