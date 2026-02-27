using AllegroService.Domain.Entities.Base;

namespace AllegroService.Domain.Entities;

public class Product : TenantEntity
{
    public Guid Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;

    public Guid CategoryId { get; set; }
    public ProductCategory Category { get; set; } = null!;

    public string Unit { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TrackStock { get; set; } = true;

    public ICollection<ChargeItem> ChargeItems { get; set; } = new List<ChargeItem>();
    public ICollection<StockBalance> StockBalances { get; set; } = new List<StockBalance>();
    public ICollection<StockMovement> StockMovements { get; set; } = new List<StockMovement>();
}
