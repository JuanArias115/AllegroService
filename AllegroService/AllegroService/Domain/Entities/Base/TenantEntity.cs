using AllegroService.Domain.Entities;

namespace AllegroService.Domain.Entities.Base;

public abstract class TenantEntity : AuditableEntity
{
    public Guid GlampingId { get; set; }
    public Glamping Glamping { get; set; } = null!;
}
