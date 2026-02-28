using System.Security.Claims;

namespace AllegroService.Api.Auth;

public static class ClaimsPrincipalExtensions
{
    public static string? TryGetFirebaseUid(this ClaimsPrincipal principal)
    {
        var value = principal.FindFirstValue(FirebaseClaimTypes.Subject);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    public static string? TryGetEmail(this ClaimsPrincipal principal)
        => principal.FindFirstValue(FirebaseClaimTypes.Email);
}
