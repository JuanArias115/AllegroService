using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class Charge : TenantEntity
{
    public Guid Id { get; set; }

    public Guid FolioId { get; set; }
    public Folio Folio { get; set; } = null!;

    public ChargeSource Source { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal Qty { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Total { get; set; }
    public string? MetadataJson { get; set; }

    public ICollection<ChargeItem> Items { get; set; } = new List<ChargeItem>();
}
