using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class Stay : TenantEntity
{
    public Guid Id { get; set; }

    public Guid? ReservationId { get; set; }
    public Reservation? Reservation { get; set; }

    public Guid UnitId { get; set; }
    public Unit Unit { get; set; } = null!;

    public DateTimeOffset CheckInAt { get; set; }
    public DateTimeOffset? CheckOutAt { get; set; }
    public StayStatus Status { get; set; } = StayStatus.CheckedIn;

    public ICollection<Folio> Folios { get; set; } = new List<Folio>();
}
