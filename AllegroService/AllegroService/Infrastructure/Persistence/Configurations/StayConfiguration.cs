using AllegroService.Domain.Entities;
using AllegroService.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class StayConfiguration : IEntityTypeConfiguration<Stay>
{
    public void Configure(EntityTypeBuilder<Stay> builder)
    {
        builder.ToTable("Stays", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Stays_CheckOutAfterCheckIn", "\"CheckOutAt\" IS NULL OR \"CheckOutAt\" > \"CheckInAt\"");
        });

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.CheckInAt)
            .IsRequired();

        builder.Property(x => x.CheckOutAt);

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.HasIndex(x => new { x.GlampingId, x.UnitId });

        builder.HasIndex(x => x.UnitId)
            .IsUnique()
            .HasFilter($"\"Status\" = {(int)StayStatus.CheckedIn}");

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Unit)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Reservation)
            .WithMany(x => x.Stays)
            .HasForeignKey(x => x.ReservationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
