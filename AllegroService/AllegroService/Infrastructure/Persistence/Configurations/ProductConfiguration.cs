using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Products_SalePriceNonNegative", "\"SalePrice\" >= 0");
        });

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.Sku)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.SalePrice)
            .ConfigureMoney();

        builder.Property(x => x.CostPrice)
            .ConfigureMoney();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.TrackStock)
            .IsRequired();

        builder.HasIndex(x => new { x.GlampingId, x.Sku })
            .IsUnique();

        builder.HasIndex(x => new { x.GlampingId, x.CategoryId });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Products)
            .HasForeignKey(x => x.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
