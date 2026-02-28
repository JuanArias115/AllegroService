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
    private readonly ILogger<ResolveTenantContextMiddleware> _logger;

    public ResolveTenantContextMiddleware(RequestDelegate next, ILogger<ResolveTenantContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, AppDbContext dbContext, ITenantContextAccessor tenantContextAccessor)
    {
        var endpoint = context.GetEndpoint();
        var allowsAnonymous = endpoint?.Metadata?.GetMetadata<Microsoft.AspNetCore.Authorization.IAllowAnonymous>() is not null;

        if (allowsAnonymous)
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true)
        {
            _logger.LogWarning("Unauthenticated request reached tenant resolution. Path: {Path}", context.Request.Path);
            throw new UnauthorizedAccessException("Invalid/unauthenticated token");
        }

        var firebaseUid = context.User.TryGetFirebaseUid();
        if (string.IsNullOrWhiteSpace(firebaseUid))
        {
            var claimTypes = string.Join(",", context.User.Claims.Select(x => x.Type).Distinct().OrderBy(x => x));
            _logger.LogWarning("Authenticated token without usable Firebase UID. Path: {Path}. Claims: {Claims}", context.Request.Path, claimTypes);
            throw new UnauthorizedAccessException("Invalid/unauthenticated token");
        }

        var userTenant = await dbContext.UserTenants.AsNoTracking()
            .FirstOrDefaultAsync(x => x.FirebaseUid == firebaseUid, context.RequestAborted);

        if (userTenant is null)
        {
            _logger.LogInformation("Authenticated Firebase UID is not onboarded. UID: {FirebaseUid}", firebaseUid);
            throw new ForbiddenException("USER_NOT_ONBOARDED");
        }

        if (userTenant.Status == UserTenantStatus.Disabled)
        {
            _logger.LogInformation("Authenticated Firebase UID is disabled. UID: {FirebaseUid}", firebaseUid);
            throw new ForbiddenException("USER_DISABLED");
        }

        if (userTenant.Status != UserTenantStatus.Active)
        {
            _logger.LogInformation("Authenticated Firebase UID is not active. UID: {FirebaseUid}. Status: {Status}", firebaseUid, userTenant.Status);
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

        await _next(context);
    }
}
