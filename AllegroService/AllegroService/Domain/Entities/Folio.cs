using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class Folio : TenantEntity
{
    public Guid Id { get; set; }

    public Guid StayId { get; set; }
    public Stay Stay { get; set; } = null!;

    public FolioStatus Status { get; set; } = FolioStatus.Open;
    public DateTimeOffset OpenedAt { get; set; }
    public DateTimeOffset? ClosedAt { get; set; }

    public ICollection<Charge> Charges { get; set; } = new List<Charge>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
