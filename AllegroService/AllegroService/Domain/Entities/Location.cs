using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class Location : TenantEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public LocationType Type { get; set; }

    public Guid? UnitId { get; set; }
    public Unit? Unit { get; set; }

    public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
