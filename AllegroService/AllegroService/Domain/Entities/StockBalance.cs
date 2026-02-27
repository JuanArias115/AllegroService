using AllegroService.Domain.Entities.Base;

namespace AllegroService.Domain.Entities;

public class StockBalance : TenantEntity
{
    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public decimal QtyOnHand { get; set; }
}
