using System.Security.Claims;
using AllegroService.Api.Auth;
using AllegroService.Application.Common;
using AllegroService.Domain.Enums;
using AllegroService.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AllegroService.Api.Middlewares;

public class ResolveTenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public ResolveTenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        var endpoint = context.GetEndpoint();
        var allowsAnonymous = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() is not null;

        if (!allowsAnonymous && context.User.Identity?.IsAuthenticated == true)
        {
            var firebaseUid = context.User.TryGetFirebaseUid();
            if (string.IsNullOrWhiteSpace(firebaseUid))
            {
                throw new UnauthorizedAccessException("Firebase UID claim (sub) is required.");
            }

            var userTenant = await dbContext.UserTenants.AsNoTracking()
                .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, context.RequestAborted);

            if (userTenant is null || userTenant.Status != UserTenantStatus.Active)
            {
                throw new ForbiddenException("USER_NOT_ONBOARDED");
            }

            tenantContextAccessor.Current = new ResolvedTenantContext(
                userTenant.Id,
                userTenant.FirebaseUid,
                userTenant.Email,
                userTenant.GlampingId,
                userTenant.Role);

            var identity = context.User.Identity as ClaimsIdentity;
            if (identity is not null)
            {
                var existingRole = identity.FindFirst(FirebaseClaimTypes.Role);
                if (existingRole is null || !string.Equals(existingRole.Value, userTenant.Role.ToString(), StringComparison.Ordinal))
                {
                    if (existingRole is not null)
                    {
                        identity.RemoveClaim(existingRole);
                    }

                    identity.AddClaim(new Claim(FirebaseClaimTypes.Role, userTenant.Role.ToString()));
                }
            }
        }

        await _next(context);
    }
}
