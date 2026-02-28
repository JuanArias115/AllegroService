using AllegroService.Api.Auth;
using AllegroService.Application.Common;

namespace AllegroService.Api.Middlewares;

public class RequireGlampingClaimMiddleware
{
    private readonly RequestDelegate _next;

    public RequireGlampingClaimMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var allowsAnonymous = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() is not null;

        if (!allowsAnonymous && context.User.Identity?.IsAuthenticated == true)
        {
            if (!context.User.TryGetGlampingId(out _))
            {
                throw new ForbiddenException("glamping_id claim is required and must be a valid GUID.");
            }
        }

        await _next(context);
    }
}
