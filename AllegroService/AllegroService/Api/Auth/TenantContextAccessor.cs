namespace AllegroService.Api.Auth;

public sealed class TenantContextAccessor : ITenantContextAccessor
{
    public ResolvedTenantContext? Current { get; set; }
}
