using AllegroService.Domain.Entities.Base;

namespace AllegroService.Domain.Entities;

public class ChargeItem : TenantEntity
{
    public Guid Id { get; set; }

    public Guid ChargeId { get; set; }
    public Charge Charge { get; set; } = null!;

    public Guid? ProductId { get; set; }
    public Product? Product { get; set; }

    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
}
