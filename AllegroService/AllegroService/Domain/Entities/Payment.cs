using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class Payment : TenantEntity
{
    public Guid Id { get; set; }

    public Guid FolioId { get; set; }
    public Folio Folio { get; set; } = null!;

    public decimal Amount { get; set; }
    public PaymentMethod Method { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public DateTimeOffset? PaidAt { get; set; }
    public string? Reference { get; set; }
}
