using AllegroService.Domain.Entities.Base;
using AllegroService.Domain.Enums;

namespace AllegroService.Domain.Entities;

public class UserTenant : AuditableEntity
{
    public Guid Id { get; set; }
    public string FirebaseUid { get; set; } = string.Empty;
    public string? Email { get; set; }
    public Guid GlampingId { get; set; }
    public UserTenantRole Role { get; set; } = UserTenantRole.Reception;
    public UserTenantStatus Status { get; set; } = UserTenantStatus.Pending;

    public Glamping Glamping { get; set; } = null!;
}
