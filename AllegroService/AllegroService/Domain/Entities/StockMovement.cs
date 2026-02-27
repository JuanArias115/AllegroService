using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class StockMovement : TenantEntity
{
    public Guid Id { get; set; }

    public Guid ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public Guid LocationId { get; set; }
    public Location Location { get; set; } = null!;

    public StockMovementType Type { get; set; }
    public decimal Qty { get; set; }
    public string? ReferenceType { get; set; }
    public Guid? ReferenceId { get; set; }
}
