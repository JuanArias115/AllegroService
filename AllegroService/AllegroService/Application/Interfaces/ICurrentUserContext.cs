using AllegroService.Domain.Enums;

namespace AllegroService.Application.Interfaces;

public interface ICurrentUserContext
{
    bool IsAuthenticated { get; }
    Guid GetRequiredGlampingId();
    string GetRequiredFirebaseUid();
    Guid? GetCurrentUserId();
    string? GetCurrentFirebaseUid();
    string? GetCurrentUserEmail();
    UserTenantRole? GetCurrentRole();
}
