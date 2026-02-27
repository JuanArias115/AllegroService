using AllegroService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AllegroService.Infrastructure.Persistence.Configurations;

public class ReservationConfiguration : IEntityTypeConfiguration<Reservation>
{
    public void Configure(EntityTypeBuilder<Reservation> builder)
    {
        builder.ToTable("Reservations", tableBuilder =>
        {
            tableBuilder.HasCheckConstraint("CK_Reservations_CheckOutAfterCheckIn", "\"CheckOutDate\" > \"CheckInDate\"");
        });

        builder.HasKey(x => x.Id);

        builder.ConfigureTenantScope();

        builder.Property(x => x.Code)
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.CheckInDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.CheckOutDate)
            .HasColumnType("date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.TotalEstimated)
            .ConfigureMoney();

        builder.HasIndex(x => new { x.GlampingId, x.Code })
            .IsUnique();

        builder.HasIndex(x => new { x.GlampingId, x.GuestId });
        builder.HasIndex(x => new { x.GlampingId, x.UnitId });

        builder.HasOne(x => x.Glamping)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.GlampingId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Guest)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.GuestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Unit)
            .WithMany(x => x.Reservations)
            .HasForeignKey(x => x.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAudit();
    }
}
