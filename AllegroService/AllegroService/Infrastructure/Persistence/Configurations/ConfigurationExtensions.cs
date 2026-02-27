using AllegroService.Domain.Entities.Base;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public static void ConfigureAudit<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : class, IAuditableEntity
    {
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.CreatedByUserId);
        builder.Property(x => x.UpdatedByUserId);
    }

    public static void ConfigureTenantScope<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : TenantEntity
    {
        builder.Property(x => x.GlampingId).IsRequired();
        builder.HasIndex(x => x.GlampingId);
    }

    public static void ConfigureMoney(this PropertyBuilder<decimal> propertyBuilder)
    {
        propertyBuilder.HasPrecision(18, 2);
    }

    public static void ConfigureMoney(this PropertyBuilder<decimal?> propertyBuilder)
    {
        propertyBuilder.HasPrecision(18, 2);
    }

    public static void ConfigureQuantity(this PropertyBuilder<decimal> propertyBuilder)
    {
        propertyBuilder.HasPrecision(18, 3);
    }

    public static void ConfigureQuantity(this PropertyBuilder<decimal?> propertyBuilder)
    {
        propertyBuilder.HasPrecision(18, 3);
    }
}
