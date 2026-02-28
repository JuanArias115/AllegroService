using AllegroService.Domain.Enums;

namespace AllegroService.Application.DTOs.UserTenants;

public sealed record UserTenantDto(
    Guid Id,
    string FirebaseUid,
    string? Email,
    Guid GlampingId,
    UserTenantRole Role,
    UserTenantStatus Status);

public class CreateUserTenantRequest
{
    public string FirebaseUid { get; set; } = string.Empty;
    public string? Email { get; set; }
    public UserTenantRole Role { get; set; } = UserTenantRole.Reception;
    public UserTenantStatus Status { get; set; } = UserTenantStatus.Active;
}

public class UpdateUserTenantRequest
{
    public string? Email { get; set; }
    public UserTenantRole Role { get; set; }
    public UserTenantStatus Status { get; set; } = UserTenantStatus.Active;
}
