using AllegroService.Api.Auth;
using AllegroService.Application.Common;
using AllegroService.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace AllegroService.Api.Auth;

public class CurrentUserContext : ICurrentUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public bool IsAuthenticated => _httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid GetRequiredGlampingId()
    {
        var user = _httpContextAccessor.HttpContext?.User;

        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new UnauthorizedAccessException("Authentication is required.");
        }

        if (!user.TryGetGlampingId(out var glampingId))
        {
            throw new ForbiddenException("glamping_id claim is required and must be a valid GUID.");
        }

        return glampingId;
    }

    public Guid? GetCurrentUserId()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.TryGetUserId();
    }

    public string? GetCurrentUserEmail()
    {
        var user = _httpContextAccessor.HttpContext?.User;
        return user?.TryGetEmail();
    }
}
