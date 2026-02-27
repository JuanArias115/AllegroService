using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class GuestConfiguration : IEntityTypeConfiguration<Guest>
{
    public void Configure(EntityTypeBuilder<Guest> builder)
    {
        builder.ToTable("Guests");

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.FullName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.DocumentId)
            .HasMaxLength(50);

        builder.Property(x => x.Phone)
            .HasMaxLength(40)
            .IsRequired();

        builder.Property(x => x.Email)
            .HasMaxLength(200)
            .IsRequired();

        builder.HasIndex(x => new { x.GlampingId, x.Email });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.Guests)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
