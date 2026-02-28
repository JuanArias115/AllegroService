using AllegroService.Api.Auth;
using AllegroService.Application.Common;
using AllegroService.Application.Interfaces;
using AllegroService.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace AllegroService.Api.Auth;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ITenantContextAccessor _tenantContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor, ITenantContextAccessor tenantContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        _tenantContextAccessor = tenantContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid GetRequiredGlampingId()
    {
        if (!IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authentication is required.");
        }

        var tenantContext = _tenantContextAccessor.Current;
        if (tenantContext is null)
        {
            throw new ForbiddenException("USER_NOT_ONBOARDED");
        }

        return tenantContext.GlampingId;
    }

    public string GetRequiredFirebaseUid()
    {
        var firebaseUid = GetCurrentFirebaseUid();
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            throw new UnauthorizedAccessException("Firebase UID is required.");
        }

        return firebaseUid;
    }

    public Guid? GetCurrentUserId()
    {
        return _tenantContextAccessor.Current?.UserTenantId;
    }

    public string? GetCurrentFirebaseUid()
    {
        var tenantContextUid = _tenantContextAccessor.Current?.FirebaseUid;
        if (!string.IsNullOrWhiteSpace(tenantContextUid))
        {
            return tenantContextUid;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        return user?.TryGetFirebaseUid();
    }

    public string? GetCurrentUserEmail()
    {
        var tenantContextEmail = _tenantContextAccessor.Current?.Email;
        if (!string.IsNullOrWhiteSpace(tenantContextEmail))
        {
            return tenantContextEmail;
        }

        var user = _httpContextAccessor.HttpContext?.User;
        return user?.TryGetEmail();
    }

    public UserTenantRole? GetCurrentRole()
    {
        return _tenantContextAccessor.Current?.Role;
    }
}
