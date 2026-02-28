using System.Security.Claims;

namespace AllegroService.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static bool TryGetGlampingId(this ClaimsPrincipal principal, out Guid glampingId)
    {
        glampingId = Guid.Empty;
        var value = principal.FindFirstValue(FirebaseClaimTypes.GlampingId);
        return Guid.TryParse(value, out glampingId);
    }

    public static Guid? TryGetUserId(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(FirebaseClaimTypes.Subject);
        return Guid.TryParse(value, out var userId) ? userId : null;
    }

    public static string? TryGetEmail(this ClaimsPrincipal principal)
        => principal.FindFirstValue(FirebaseClaimTypes.Email);
}
