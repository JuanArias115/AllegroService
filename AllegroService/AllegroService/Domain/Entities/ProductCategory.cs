using AllegroService.Domain.Entities.Base;

namespace AllegroService.Domain.Entities;

public class ProductCategory : TenantEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public ICollection<Product> Products { get; set; } = new List<Product>();
}
