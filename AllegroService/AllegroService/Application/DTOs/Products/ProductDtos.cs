namespace AllegroService.Application.DTOs.Products;

public sealed record ProductDto(
    Guid Id,
    string Sku,
    string Name,
    Guid CategoryId,
    string CategoryName,
    string Unit,
    decimal SalePrice,
    decimal? CostPrice,
    bool IsActive,
    bool TrackStock);

public class CreateProductRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public bool IsActive { get; set; } = true;
    public bool TrackStock { get; set; } = true;
}

public class UpdateProductRequest
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid CategoryId { get; set; }
    public string Unit { get; set; } = string.Empty;
    public decimal SalePrice { get; set; }
    public decimal? CostPrice { get; set; }
    public bool IsActive { get; set; }
    public bool TrackStock { get; set; }
}
