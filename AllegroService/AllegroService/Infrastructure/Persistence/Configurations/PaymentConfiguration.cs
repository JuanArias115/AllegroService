using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Payments_AmountPositive", "\"Amount\" > 0");
        });

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.Amount)
            .ConfigureMoney();

        builder.Property(x => x.Method)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Reference)
            .HasMaxLength(100);

        builder.HasIndex(x => new { x.GlampingId, x.FolioId });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Folio)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.FolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
