using AllegroService.Domain.Enums;

namespace AllegroService.Api.Auth;

public sealed record ResolvedTenantContext(
    Guid UserTenantId,
    string FirebaseUid,
    string? Email,
    Guid GlampingId,
    UserTenantRole Role);
