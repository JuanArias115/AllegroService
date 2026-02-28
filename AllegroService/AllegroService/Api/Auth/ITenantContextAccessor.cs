namespace AllegroService.Api.Auth;

public interface ITenantContextAccessor
{
    ResolvedTenantContext? Current { get; set; }
}
