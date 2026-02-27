using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class Reservation : TenantEntity
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;

    public Guid GuestId { get; set; }
    public Guest Guest { get; set; } = null!;

    public Guid? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public DateOnly CheckInDate { get; set; }
    public DateOnly CheckOutDate { get; set; }
    public ReservationStatus Status { get; set; } = ReservationStatus.Pending;
    public decimal TotalEstimated { get; set; }

    public ICollection<Stay> Stays { get; set; } = new List<Stay>();
}
